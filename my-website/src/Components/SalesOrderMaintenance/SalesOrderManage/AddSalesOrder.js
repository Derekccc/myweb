import React, { useState, useRef, useEffect } from 'react';
import { useCookies } from 'react-cookie';
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import 'react-toastify/dist/ReactToastify.min.css';
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { BiSolidPurchaseTag } from "react-icons/bi";
import { MdPerson, MdAttachMoney, MdDateRange, MdOutlineMonitorHeart } from "react-icons/md";
import { TbNumbers } from "react-icons/tb";
import { AiFillProduct } from "react-icons/ai";

const AddSalesOrder = (props) => {
    const [cookies] = useCookies([]);
    const [userId] = useState(cookies.USER_ID);
  
    const quantityInputRef = useRef();
    const totalAmountInputRef = useRef();
    const orderDateTimeInputRef = useRef();
    const orderStatusInputRef = useRef();

    const [customerListToBeSelect, setCustomerListToBeSelect] = useState([]);
    const [customerList, setCustomerList] = useState([]);

    const [productListToBeSelect, setProductListToBeSelect] = useState([]);
    const [productList, setProductList] = useState([]);

    const [selectedProduct, setSelectedProduct] = useState(null);
    const [availableQuantity, setAvailableQuantity] = useState(0);
    const [unitSellingPrice, setUnitSellingPrice] = useState(0.00);
    
    const [addData, setAddData] = useState({
        TOTAL_AMOUNT: '',
        ORDER_STATUS: 'New Request Sales Order',
    });

    const [inputError, setInputError] = useState({});

    //#region Fetch Customers List
    useEffect(() => {
        let functionName = "";
        try {
            functionName = "Get Customer List";
            http
            .get("api/salesOrder/GetCustomerList?_sysUser=" + userId)
            .then((response) => {
               const customerlist = response.data.map(customer => ({
                key: customer.CUSTOMER_ID,
                value: customer.CUSTOMER_ID,
                label: `${customer.CUSTOMER_NAME}`
                // (${customer.CUSTOMER_ID})`
               }));
                  setCustomerListToBeSelect(customerlist);
                })
            .catch((err) => {
                common.c_LogWebError(props.page, functionName, err);
            })
        } catch (err) {
            common.c_LogWebError(props.page, functionName, err);
        }
    }, []);
    //endregion

    //#region Fetch Products List
    useEffect(() => {
        let functionName = "";
        try {
            functionName = "Get Product List";
            http
            .get("api/salesOrder/GetProductList?_sysUser=" + userId)
            .then((response) => {
                const productlist = response.data.map(product => ({
                    key:product.PRODUCT_ID,
                    value:product.PRODUCT_ID,
                    label:product.PRODUCT_NAME,
                    availableQuantity:product.AVAILABLE_QUANTITY,
                }));
                  setProductListToBeSelect(productlist);
                  setProductList(response.data);
                })
                .catch((err) => {
                    common.c_LogWebError(props.page, functionName, err);
                })
        } catch (err) {
            common.c_LogWebError(props.page, functionName, err);
        }
    }, []);
    //endregion

    //#region Select Customer Handle
    const selectCustomerHandler = (val) => {
        let functionName = "";
    
        try {
          functionName = selectCustomerHandler.name;
          if (Object.keys(val).length > 0) {
            setInputError((prevState) => ({
              ...prevState,
              CUSTOMER: (""),
            }));
          } else {
            setInputError((prevState) => ({
              ...prevState,
              CUSTOMER: ("Customer could not be empty."),
            }));
          }
          setCustomerList([val]);
          setAddData((prevState) => ({
            ...prevState,
            CUSTOMER_ID: val.value,
            CUSTOMER_NAME: val.label,
          }));
        } catch (err) {
          common.c_LogWebError(props.page, functionName, err);
        }
      }
    //#endregion

    //#region Select Product Handle
    const selectProductHandler = (val) => {
        let functionName = "";
    
        try {
          functionName = selectProductHandler.name;
          if (Object.keys(val).length > 0) {
            setInputError((prevState) => ({
              ...prevState,
              PRODUCT: (""),
            }));
            const selectedProd = productList.find(product => product.PRODUCT_ID === val.value);
            setSelectedProduct(val);
            setAvailableQuantity(selectedProd ? selectedProd.AVAILABLE_QUANTITY : 0);
            setUnitSellingPrice(selectedProd ? selectedProd.UNIT_SELLING_PRICE : 0.00);
            setAddData((prevState) => ({
                ...prevState,
                PRODUCT_ID: val.value,
                PRODUCT_NAME: val.label,
                UNIT_SELLING_PRICE: selectedProd ? selectedProd.UNIT_SELLING_PRICE : 0.00,
                TOTAL_AMOUNT: (addData.QUANTITY * (selectedProd ? selectedProd.UNIT_SELLING_PRICE : 0.00)).toFixed(2)
            }));
          } else {
            setInputError((prevState) => ({
              ...prevState,
              PRODUCT: ("Product could not be empty."),
            }));
            setSelectedProduct(null);
            setAvailableQuantity(0);
            setUnitSellingPrice(0.00);
            setAddData((prevState) => ({
                ...prevState,
                PRODUCT_ID: '',
                PRODUCT_NAME: '',
                UNIT_SELLING_PRICE: 0.00,
                TOTAL_AMOUNT: '',
            }));
          }
        } catch (err) {
          common.c_LogWebError(props.page, functionName, err);
        }
      }
    //#endregion

    //#region Event Listener
    const hideModal = () => {
        setAddData({});
        setInputError({});
        props.onHide();
    };
    //#endregion

    //#region Onclick Function
    const saveBtnOnClick = () => {
        var flag = true;

        if (Object.keys(customerList).length === 0) {
            flag = false;
            setInputError((prevState) => ({
              ...prevState,
              CUSTOMER: ("Customer could not be empty."),
            }));
          } else {
            setInputError((prevState) => ({
              ...prevState,
              CUSTOMER: (""),
            }));
          }

        if (!selectedProduct) {
            flag = false;
            setInputError((prevState) => ({
              ...prevState,
              PRODUCT: ("Product could not be empty."),
            }));
          } else {
            setInputError((prevState) => ({
              ...prevState,
              PRODUCT: (""),
            }));
          }

        if (!addData.ORDER_DATETIME || addData.ORDER_DATETIME.length === 0) {
            flag = false;
            setInputError((prevState) => ({
                ...prevState,
                ORDER_DATETIME: "Order date cannot be empty.",
            }));
        } else {
            setInputError((prevState) => ({
                ...prevState,
                ORDER_DATETIME: "",
            }));
        }

        if (!addData.QUANTITY || isNaN(addData.QUANTITY) || parseFloat(addData.QUANTITY) <= 0) {
            flag = false;
            setInputError((prevState) => ({
                ...prevState,
                QUANTITY: "Quantity must be a positive number and cannot be zero.",
            }));
        } else if (parseFloat(addData.QUANTITY) > availableQuantity) {
            flag = false;
            setInputError((prevState) => ({
                ...prevState,
                QUANTITY: "Quantity cannot exceed available stock.",
            }));
        } else {
            setInputError((prevState) => ({
                ...prevState,
                QUANTITY: "",
            }));
        }

        if (Object.values(inputError).filter(v => v !== "").length === 0 && flag) {
            RegisterNewSalesOrder();
        }
    };

    const RegisterNewSalesOrder = () => {
        try {
            props.onLoading(true, "Registering sales order, please wait...");
            
            const data = {
                CUSTOMER_ID: addData.CUSTOMER_ID.trim(),
                PRODUCT_ID: addData.PRODUCT_ID.trim(),
                TOTAL_AMOUNT: addData.TOTAL_AMOUNT ? parseFloat(addData.TOTAL_AMOUNT).toFixed(2) : 0.00,
                QUANTITY: addData.QUANTITY,
                ORDER_DATETIME: addData.ORDER_DATETIME,
                ORDER_STATUS: addData.ORDER_STATUS,
                UPDATE_ID: userId,
            };

            http
                .post("api/salesOrder/InsertSalesOrder", data, { timeout: 100000 })
                .then((response) => {
                    
                    // Deduct quantity from the selected product
                    const updatedProductList = productList.map(product => {
                        if (product.PRODUCT_ID === addData.PRODUCT_ID) {
                            return {
                                ...product,
                                AVAILABLE_QUANTITY: product.AVAILABLE_QUANTITY - (addData.QUANTITY)
                            };
                        }
                        console.log("product ID");
                        return product;
                        
                    });

                    // Update state with the updated product list
                    setProductList(updatedProductList);

                    toast.success("Sales order is successfully inserted.");
                    props.onReload();
                    hideModal();
                })
                .catch((err) => {
                    toast.error("Failed to insert sales order. Please try again.");
                    common.c_LogWebError(props.page, "RegisterNewSalesOrder", err);
                })
                .finally(() => {
                    props.onLoading(false, "Loading...");
                });
        } catch (err) {
            toast.error("Failed to insert sales order. Please try again.");
            common.c_LogWebError(props.page, "RegisterNewSalesOrder", err);
        }
        console.log("Data being sent to backend:", addData);
    };
    //#endregion

    const inputRequiredHandler = (event) => {
        const name = event.target.name;
        const id = event.target.id;
        const val = event.target.value;
        if (val.length === 0 || val.trim().length === 0) {
          setInputError((prevState) => ({
            ...prevState,
            [id]: `${name} could not be empty.`,
          }));
        } else {
          setInputError((prevState) => ({
            ...prevState,
            [id]: "",
          }));
        }
        setAddData((prevState) => ({
          ...prevState,
          [id]: val,
        }));
      };

    //#region Date Time HANDLE
    const handleDateTimeChange = (event) => {
        const date = event.target.value;
        setAddData((prevState) => ({
            ...prevState,
            ORDER_DATETIME: date,
        }));
        if (date) {
            setInputError((prevState) => ({
                ...prevState,
                ORDER_DATETIME: "",
            }));
        }
    }
    
    //#endregion

    //#region Qtt/TAmount HANDLE
    const quantityInputHandler = (event) => {
        const { value } = event.target;
        const quantity = parseFloat(value);
        
        if (isNaN(quantity) || quantity <= 0) {
            setInputError((prevState) => ({
                ...prevState,
                QUANTITY: "Quantity must be a positive number and cannot be zero.",
            }));
        } else if (quantity > availableQuantity) {
            setInputError((prevState) => ({
                ...prevState,
                QUANTITY: "Quantity cannot exceed available stock.",
            }));
        } else {
            setInputError((prevState) => ({
                ...prevState,
                QUANTITY: "",
            }));
        }
        
        setAddData((prevState) => ({
            ...prevState,
            QUANTITY: value,
            TOTAL_AMOUNT: quantity > 0 ? (quantity * parseFloat(unitSellingPrice).toFixed(2)) : 0.00,
        }));
    };

    // const totalAmountFormatted = parseFloat(addData.TOTAL_AMOUNT).toFixed(2);
    
    //#endregion

    const ColorAvailableQuantity = () => {
        if(!selectedProduct) {
            return null;
        }
        else if (availableQuantity === 0) {
            return <p style={{ color: 'red' }}>** Available Quantity: {availableQuantity}</p>;
        } 
        else {
            return <p style={{ color: 'green' }}>* Available Quantity: {availableQuantity}</p>;
        }
    };

    return (
        <Modal show={props.showHide} onHide={hideModal} centered>
            <Modal.Header closeButton>
                <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>
                    <Modal.Title>ADD NEW SALES ORDER &nbsp; <BiSolidPurchaseTag className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px' }}/></Modal.Title>
                </div>
            </Modal.Header>
            <Modal.Body>
                <div>
                    <MdPerson className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
                    <label label="Customer">Customer:</label>
                    <Comp.Select
                        id="CUSTOMER"
                        name="Customer"
                        type="text"
                        closeMenuOnSelect={true}
                        hideSelectedOptions={false}
                        errorMessage={inputError.CUSTOMER}
                        value={customerList}
                        onChange={selectCustomerHandler}
                        options={customerListToBeSelect}
                        isMulti = {false}
                    />
                </div>
                <br/>
                <div>
                    <AiFillProduct className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
                    <label label="Product">Product:</label>
                    <Comp.Select
                        id="PRODUCT"
                        name="Product"
                        type="text"
                        closeMenuOnSelect={true}
                        hideSelectedOptions={false}
                        errorMessage={inputError.PRODUCT}
                        value={selectedProduct}
                        onChange={selectProductHandler}
                        options={productListToBeSelect}
                        isMulti = {false}
                    />
                    {ColorAvailableQuantity()}
                    {selectedProduct && (
                        <p style={{ color: 'black' }}>$ Unit Selling Price: RM {unitSellingPrice.toFixed(2)}</p>
                    )}
                </div>
                <br/>
                <div>
                    <TbNumbers className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
                    <label htmlFor="QUANTITY">Quantity:</label>
                    <Comp.Input
                        ref={quantityInputRef}
                        id="QUANTITY"
                        name="Quantity"
                        type="text"
                        placeholder="Enter Quantity Amount"
                        className="form-control"
                        errorMessage={inputError.QUANTITY}
                        value={addData.QUANTITY}
                        onChange={quantityInputHandler}
                        disabled={!selectedProduct}
                    />
                </div>
                <div>
                    <MdAttachMoney className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
                    <label htmlFor="TOTAL_AMOUNT">Total Amount (RM):</label>
                    <Comp.Input
                        ref={totalAmountInputRef}

                        id="TOTAL_AMOUNT"
                        name="Total Amount"
                        type="text"
                        className="form-control"
                        errorMessage={inputError.TOTAL_AMOUNT}
                        value={addData.TOTAL_AMOUNT ? `RM ${parseFloat(addData.TOTAL_AMOUNT).toFixed(2)}` : "RM 0.00"}
                        readOnly = {true}
                    />
                </div>
                <div>
                    <MdDateRange className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
                    <label htmlFor="ORDER_DATETIME">Order Date and Time:</label>
                    <Comp.Input
                        ref={orderDateTimeInputRef}
                        id="ORDER_DATETIME"
                        name="Order Date and Time"
                        type="date"
                        className="form-control"
                        errorMessage={inputError.ORDER_DATETIME}
                        value={addData.ORDER_DATETIME}
                        onChange={handleDateTimeChange}
                    />
                </div>
                <div>
                    <MdOutlineMonitorHeart className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
                    <label htmlFor="ORDER_STATUS">Order Status:</label>
                    <Comp.Input
                        ref={orderStatusInputRef}
                        id="ORDER_STATUS"
                        name="Order Status"
                        className="form-control"
                        errorMessage={inputError.ORDER_STATUS}
                        value={addData.ORDER_STATUS}
                        onChange={inputRequiredHandler}
                        readOnly={true}
                    />
                </div>
            </Modal.Body>
            <Modal.Footer>
                <div style={{ textAlign: "center", width: "100%" }}>
                    <Comp.Button
                        id="btnCancel"
                        type="cancel"
                        onClick={hideModal}
                    >
                        Cancel
                    </Comp.Button>
                    &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                    <Comp.Button 
                        id="btnSave" 
                        onClick={saveBtnOnClick}
                    >
                        ADD
                    </Comp.Button>
                </div>
            </Modal.Footer>
        </Modal>
    );
};

export default AddSalesOrder;
