import React, { useState, useEffect, useRef } from 'react';
import { toast } from 'react-toastify';
import { useCookies } from 'react-cookie';
import http from '../../../Common/http-common';
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { Modal } from "react-bootstrap";
import { GrUpdate } from "react-icons/gr";
import { BiSolidPurchaseTag } from "react-icons/bi";
import { SiNamemc, SiInstatus } from "react-icons/si";
import { FcSalesPerformance } from "react-icons/fc";

const SalesOrderUpdate = (props) => {
  //#region React Hooks
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const salesOrderIdInputRef = useRef();
  const customerNameInputRef = useRef();
  const totalAmountInputRef = useRef();
  const orderStatusInputRef = useRef();

  const [editData, setEditData] = useState([]);
  const [inputError, setInputError] = useState([]);

  const orderStatusOptions = [
    { value: 'Pending', label: 'Pending' },
    { value: 'Processing', label: 'Processing' },
    { value: 'Sending', label: 'Sending' },
    { value: 'Delivery', label: 'Delivery' },
    { value: 'Complete', label: 'Complete' },
  ];

  useEffect(() => {
      setEditData({
        SALES_ORDER_ID: props.editData !== null ? (props.editData.SALES_ORDER_ID || "") : "",
        CUSTOMER_ID: props.editData !== null ? (props.editData.CUSTOMER_ID || "") : "",
        CUSTOMER_NAME: props.editData !== null ? (props.editData.CUSTOMER_NAME || "") : "",
        TOTAL_AMOUNT: props.editData !== null ? (props.editData.TOTAL_AMOUNT || 0.00) : 0.00,
        ORDER_STATUS: props.editData !== null ? (props.editData.ORDER_STATUS || "Pending") : "Pending",
      });
  }, [props.editData]);
  //#endregion

  //#region Modal Show/Hide
  const hideModal = () => {
    setInputError([]);
    props.onHide();
  };
  //#endregion

  const saveBtnOnClick = () => {
    if (Object.values(inputError).filter(v => v !== "").length === 0) {
      saveEditSalesOrder();
    }
  }

  //#region Save Edit
  function saveEditSalesOrder() {
    let functionName = "";
    try {
      functionName = saveEditSalesOrder.name;
      props.onLoading(true, "Updating sales order, Please wait....");

      const data = {
        SALES_ORDER_ID: editData.SALES_ORDER_ID.trim(),
        CUSTOMER_ID: editData.CUSTOMER_ID.trim(),
        CUSTOMER_NAME: editData.CUSTOMER_NAME.trim(),
        TOTAL_AMOUNT: parseFloat(editData.TOTAL_AMOUNT).toFixed(2),
        ORDER_STATUS: editData.ORDER_STATUS,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
        UPDATE_ID: userId,
      };

      http
        .put("api/salesOrder/UpdateSalesOrder", data, { timeout: 10000 })
        .then((response) => {
          toast.success("Sales Order is successfully updated.");
          props.onReload();
          hideModal();
        })
        .catch((err) => {
          toast.error("Failed to update sales order. Please try again.1");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading....");
        });
    } catch (err) {
      props.onLoading(false, "Loading...");
      toast.error("Failed to update sales order. Please try again.");
      common.c_LogWebError(props.page, functionName, err);
    }
  };
  //#endregion

  const inputRequiredHandler = (event) => {
    const { name, id, value } = event.target;
    setInputError((prevState) => ({
      ...prevState,
      [id]: value.length === 0 ? `${name} cannot be empty` : ""
    }));
    setEditData((prevState) => ({
      ...prevState,
      [id]: value.trim(),
    }));
  };
  

  // const inputUnrequiredHandler = (event) => {
  //   const { id, value } = event.target;
  //   setEditData((prevState) => ({
  //     ...prevState,
  //     [id]: value
  //   }));
  // }

  const orderStatusChangeHandler = (selectedOption) => {
    const val = selectedOption.value;
    setEditData((prevState) => ({
      ...prevState,
      ORDER_STATUS: val,
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
            <BiSolidPurchaseTag className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
            <label label="Sales Order ID">Sales Order ID :</label>
            <Comp.Input
              ref={salesOrderIdInputRef}
              id="SALES_ORDER_ID"
              type="text"
              className="form-control"
              value={editData.SALES_ORDER_ID}
              readOnly={true}
            />
          </div>
          <div>
            <SiNamemc className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
            <label label="Customer Name">Customer Name :</label>
            <Comp.Input
              ref={customerNameInputRef}
              id="CUSTOMER_NAME"
              name="Customer Name"
              type="text"
              className="form-control"
              value={editData.CUSTOMER_NAME}
              readOnly={true}
            />
          </div>
          <div>
            <FcSalesPerformance className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
            <label label="Total Amount">Total Sales Amount / RM:</label>
            <Comp.Input
              ref={totalAmountInputRef}
              id="TOTAL_AMOUNT"
              name="Total Amount"
              type="text"
              className="form-control"
              // errorMessage={inputError.TOTAL_AMOUNT}
              value={editData.TOTAL_AMOUNT}
              // onChange={inputRequiredHandler}
              readOnly
            />
          </div>
          <div>
            <SiInstatus className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
            <label label="Order Status">Order Status :</label>
            <Comp.Select
              ref={orderStatusInputRef}
              id="ORDER_STATUS"
              name="Order Status"
              className="form-control"
              value={{ value: editData.ORDER_STATUS, label: editData.ORDER_STATUS }}
              onChange={orderStatusChangeHandler}
              options={orderStatusOptions}
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
              Save
            </Comp.Button>
          </div>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default SalesOrderUpdate;
