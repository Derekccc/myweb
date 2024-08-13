import React, { useState, useRef } from 'react';
import { useCookies } from 'react-cookie';
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import 'react-toastify/dist/ReactToastify.min.css';
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { SiMaterialformkdocs } from "react-icons/si";
import { MdDescription } from "react-icons/md";
import { GrMoney } from "react-icons/gr";
import { GiMoneyStack } from "react-icons/gi";
import { MdOutlineProductionQuantityLimits } from "react-icons/md";


const ProductRegister = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const productNameInputRef = useRef();
  const productDescInputRef = useRef();
  const unitCostInputRef = useRef();
  const unitSellingPriceInputRef = useRef();
  const quantityInputRef = useRef();

  const [addData, setAddData] = useState({});
  const [inputError, setInputError] = useState({});

  const hideModal = () => {
    setAddData({});
    setInputError({});
    props.onHide();
  };

  const saveBtnOnClick = () => {
    let flag = true;
    if (addData.PRODUCT_NAME === undefined) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        DEPARTMENT_NAME: "Product name could not be empty.",
      }));
    }
    if (addData.UNIT_COST === undefined || isNaN(addData.UNIT_COST) || parseFloat(addData.UNIT_COST) <= 0) {
        flag = false;
        setInputError((prevState) => ({
            ...prevState,
            UNIT_COST: "Unit cost must be a positive number.",
        }));
    }
    if (addData.UNIT_SELLING_PRICE === undefined || isNaN(addData.UNIT_SELLING_PRICE) || parseFloat(addData.UNIT_SELLING_PRICE) <= 0) {
      flag = false;
      setInputError((prevState) => ({
          ...prevState,
          UNIT_SELLING_PRICE: "Unit selling price must be a positive number.",
      }));
  }
    if (addData.QUANTITY === undefined || isNaN(addData.QUANTITY) || !Number.isInteger(parseFloat(addData.QUANTITY)) || parseInt(addData.QUANTITY) < 0) {
        flag = false;
        setInputError((prevState) => ({
            ...prevState,
            QUANTITY: "Quantity must be a non-negative integer.",
        }));
    }
    if (flag) {
      RegisterNewProduct();
    }
  };

  function RegisterNewProduct() {
    let functionName = "";

    try {
      functionName = props.page + RegisterNewProduct.name;
      props.onLoading(true, "Registering product, please wait...");

      const data = {
        PRODUCT_NAME: addData.PRODUCT_NAME.trim(),
        PRODUCT_DESC: addData.PRODUCT_DESC ? addData.PRODUCT_DESC.trim() : "",
        UNIT_COST: addData.UNIT_COST ? parseFloat(addData.UNIT_COST).toFixed(2) : 0.00,
        UNIT_SELLING_PRICE: addData.UNIT_SELLING_PRICE ? parseFloat(addData.UNIT_SELLING_PRICE).toFixed(2) : 0.00,
        QUANTITY: addData.QUANTITY ? parseInt(addData.QUANTITY) : 0,
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
      };

      http
        .post("api/product/InsertProduct", data, { timeout: 10000 })
        .then((response) => {
          if (response.data.DUPLICATE_PRODUCT_NAME) {
            toast.error("Product name already exists.");
          } else {
            toast.success("Product is successfully inserted.");
            props.onReload();
            hideModal();
          }
        })
        .catch((err) => {
          toast.error("Failed to insert product. Please try again.");
          common.c_LogWebError(props.page, "RegisterNewProduct", err);
        })
        .finally(() => {
          props.onLoading(false, "Loading...");
        });
    } catch (err) {
      toast.error("Failed to insert product. Please try again.");
      common.c_LogWebError(props.page, "RegisterNewProduct", err);
    }
  };

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

  const inputUnrequiredHandler = (event) => {
    const id = event.target.id;
    const val = event.target.value;
    setAddData((prevState) => ({
      ...prevState,
      [id]: val,
    }));
  };

  return (
    <Modal show={props.showHide} onHide={hideModal} centered>
      <Modal.Header closeButton>
        <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}> 
          <Modal.Title>ADD NEW PRODUCT &nbsp; <SiMaterialformkdocs className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
        </div>
      </Modal.Header>
      <Modal.Body>
        <div>
        <SiMaterialformkdocs className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
        <label label="Product Name">Product Name *:</label>
        <Comp.Input
          ref={productNameInputRef}
          id="PRODUCT_NAME"
          name="Product Name"
          type="text"
          placeholder="Enter New Product Name"
          className="form-control"
          errorMessage={inputError.PRODUCT_NAME}
          value={addData.PRODUCT_NAME}
          onChange={inputRequiredHandler}
        />
        </div>
        <div>
        <MdDescription className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
        <label label="Description:">Description:</label>
        <Comp.Input
          ref={productDescInputRef}
          id="PRODUCT_DESC"
          type="text"
          placeholder="Enter Product Description"
          className="form-control"
          value={addData.PRODUCT_DESC}
          onChange={inputUnrequiredHandler}
        />
        </div>
        <div>
        <GrMoney className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
        <label label="Unit Cost">Cost per Unit (RM / Unit) *:</label>
        <Comp.Input
          ref={unitCostInputRef}
          id="UNIT_COST"
          type="text"
          placeholder="Set Product Cost / Unit"
          className="form-control"
          value={addData.UNIT_COST}
          onChange={inputRequiredHandler}
        />
        </div>
        <div>
        <GiMoneyStack className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
        <label label="Unit Selling Price">Selling Price per Unit (RM / Unit) *:</label>
        <Comp.Input
          ref={unitSellingPriceInputRef}
          id="UNIT_SELLING_PRICE"
          type="text"
          placeholder="Set Product Selling Price / Unit"
          className="form-control"
          value={addData.UNIT_SELLING_PRICE}
          onChange={inputRequiredHandler}
        />
        </div>
        <div>
        <MdOutlineProductionQuantityLimits className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
        <label label="Quantity">Quantity *:</label>
        <Comp.Input
          ref={quantityInputRef}
          id="QUANTITY"
          type="text"
          placeholder="Set Product Quantity"
          className="form-control"
          value={addData.QUANTITY}
          onChange={inputRequiredHandler}
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

export default ProductRegister;







