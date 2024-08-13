import { useEffect, useRef, useState } from "react";
import { useCookies } from "react-cookie";
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { SiMaterialformkdocs } from "react-icons/si";
import { MdDescription } from "react-icons/md";
import { GrUpdate } from "react-icons/gr";
import { GrMoney } from "react-icons/gr";
import { GiMoneyStack } from "react-icons/gi";
import { MdOutlineProductionQuantityLimits } from "react-icons/md";


const ProductUpdate = (props) => {
  //#region React Hooks
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const productNameInputRef = useRef();
  const productDescInputRef = useRef();
  const unitCostInputRef = useRef();
  const unitSellingPriceInputRef = useRef();
  const quantityInputRef = useRef();

  const [editData, setEditData] = useState([]);
  const [inputError, setInputError] = useState([]);

  useEffect(() => {
    setEditData({
      PRODUCT_NAME: props.editData !== null ? (props.editData.PRODUCT_NAME || "") : "",
      PRODUCT_DESC: props.editData !== null ? (props.editData.PRODUCT_DESC || "") : "",
      UNIT_COST: props.editData !== null ? (props.editData.UNIT_COST || 0.00) : 0.00, 
      UNIT_SELLING_PRICE: props.editData !== null ? (props.editData.UNIT_SELLING_PRICE || 0.00) : 0.00, 
      QUANTITY: props.editData !== null ? (props.editData.QUANTITY || 0) : 0,
      PRODUCT_ID: props.editData ? (props.editData.PRODUCT_ID || "") : "",
    });
  }, [props.editData]);

   //#endregion

  //#region Modal Show/Hide
  const hideModal = () => {
    setInputError({});
    props.onHide();
  };
  //#endregion

  //#region Save Edit
  const editBtnOnClick = () => {
    if (Object.values(inputError).filter(v => v !== "").length === 0) {
      SaveEditProduct();
    }
  };

  function SaveEditProduct() {
    let functionName = "";

    try {
      functionName = props.page + SaveEditProduct.name;

      props.onLoading(true, "Modifying product, please wait...");

      const data = {
        PRODUCT_ID: editData.PRODUCT_ID,
        PRODUCT_NAME: editData.PRODUCT_NAME.trim(),
        PRODUCT_DESC: editData.PRODUCT_DESC.trim(),
        UNIT_COST: parseFloat(editData.UNIT_COST).toFixed(2),
        UNIT_SELLING_PRICE: parseFloat(editData.UNIT_SELLING_PRICE).toFixed(2),
        QUANTITY: parseInt(editData.QUANTITY),
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
      };

      http
        .put("api/product/UpdateProduct", data, { timeout: 10000 })
        .then((response) => {
          if (response.data.DUPLICATE_PRODUCT_NAME) {
            toast.error("Product name already exists.");
          } else {
            toast.success("Product successfully updated.");
            props.onReload();
            hideModal();
          }
        })
        .catch((err) => {
          toast.error("Failed to update product. Please try again.");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading...");
        });
    } catch (err) {
      props.onLoading(false, "Loading...");
      toast.error("Failed to update product. Please try again.");
      common.c_LogWebError(props.page, functionName, err);
    }
  }
  //#endregion
  
  const inputRequiredHandler = (event) => {
    const { name, id, value } = event.target;
    setInputError((prevState) => ({
      ...prevState,
      [id]: value.length === 0 ? `${name} cannot be empty` : ""
    }));
    setEditData((prevState) => ({
      ...prevState,
      [id]: value
    }));
  }
  
  const inputUnrequiredHandler = (event) => {
    const { id, value } = event.target;
    setEditData((prevState) => ({
      ...prevState,
      [id]: value
    }));
  }

  return (
    <>
      <Modal show={props.showHide} onHide={hideModal} centered>
        <Modal.Header closeButton>
          <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>
            <Modal.Title>UPDATE &nbsp; <GrUpdate className='icon' style={{ fontSize: '20px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
          </div>
        </Modal.Header>
        <Modal.Body>
          <div>
          <SiMaterialformkdocs className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Product Name">Product Name:</label>
          <Comp.Input
            ref={productNameInputRef}
            id="PRODUCT_NAME"
            name="Product Name"
            type="text"
            className="form-control"
            errorMessage={inputError.PRODUCT_NAME}
            value={editData.PRODUCT_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <MdDescription className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Decription">Decription:</label>
          <Comp.Input
            ref={productDescInputRef}
            id="PRODUCT_DESC"
            type="text"
            className="form-control"
            value={editData.PRODUCT_DESC}
            onChange={inputUnrequiredHandler}
          />
          </div>
          <div>
          <GrMoney className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Unit Cost">Cost per Unit (RM / Unit):</label>
          <Comp.Input
            ref={unitCostInputRef}
            id="UNIT_COST"
            name="Unit Cost"
            type="text"
            className="form-control"
            errorMessage={inputError.UNIT_COST}
            value={editData.UNIT_COST}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <GiMoneyStack className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Unit Selling Price">Selling Price per Unit (RM / Unit):</label>
          <Comp.Input
            ref={unitSellingPriceInputRef}
            id="UNIT_SELLING_PRICE"
            name="Unit Selling price"
            type="text"
            className="form-control"
            errorMessage={inputError.UNIT_SELLING_PRICE}
            value={editData.UNIT_SELLING_PRICE}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <MdOutlineProductionQuantityLimits className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Quantity">Quantity:</label>
          <Comp.Input
            ref={quantityInputRef}
            id="QUANTITY"
            name="Quantity"
            type="text"
            className="form-control"
            errorMessage={inputError.QUANTITY}
            value={editData.QUANTITY}
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
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <Comp.Button 
              id="btnSave" 
              onClick={editBtnOnClick}
            >
              Save
            </Comp.Button>
          </div>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ProductUpdate;