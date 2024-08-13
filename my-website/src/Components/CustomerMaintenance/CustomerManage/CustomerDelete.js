import { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { useCookies } from 'react-cookie';
import { Modal } from 'react-bootstrap';
import http from '../../../Common/http-common';
import * as common from '../../../Common/common';
import * as Comp from '../../../Common/CommonComponents'
import { MdOutlineDeleteSweep } from "react-icons/md";


const CustomerDelete = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  
  const [confirmDelete, setConfirmDelete] = useState(false);

  useEffect(() => {
    if (props.editData) {
      setConfirmDelete(true);
    }
  }, [props.editData]);

  const deleteBtnOnClick = () => {
    RemoveCustomer(props.editData);
  };

  const RemoveCustomer = (_data) => {
    let functionName = "";

    try {
      functionName = props.page + RemoveCustomer.name;

      props.onLoading(true, "Deleting Customer, Please wait....");
      props.onHide();

      const data = {
        CUSTOMER_ID: _data.CUSTOMER_ID,
        CUSTOMER_NAME: _data.CUSTOMER_NAME,
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
      };

      http  
        .put("api/customer/DeleteCustomer", data, {timeout: 5000})
        .then(() =>  {
          toast.success("Customer is successfully deleted.");
          props.onReload();
        })
        .catch((err) => {
          toast.error("Failed to delete customer. Please try again. (send)");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading....");
        });
    } catch (err) {
      props.onLoading(false, "Loading....");
      toast.error("Failed to delete customer. PLease try again. (received");
      common.c_LogWebError(props.page, functionName, err);
    }
  };

  const handleCancel = () => {
    setConfirmDelete(false);
    props.onHide();
  }

  return (
    <>
      {confirmDelete && (
        <Modal show={confirmDelete} onHide={handleCancel}>
          <Modal.Header closeButton>
            <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>
              <Modal.Title>Confirm Delete &nbsp; <MdOutlineDeleteSweep className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
            </div>
          </Modal.Header>
          <Modal.Body>
            <p style={{color: 'red'}}><b>Are you sure want to "DELETE" this customer ?</b></p>
            <hr/>
            {/* <p><b>Customer ID :</b></p>
            <p><b>{"---> ( " + ( props.editData.CUSTOMER_ID) + " ) "}</b></p> */}
            <p><b>Customer Name : {<b> {props.editData !== null && props.editData.CUSTOMER_NAME}</b>}</b></p>
            {/* <p><b>{"---> ( " + ( props.editData.CUSTOMER_NAME) + " ) "}</b></p> */}
            <Modal.Footer>
              <Comp.Button 
                variant="secondary" 
                type='cancel'
                onClick={handleCancel}
              >
                Cancel
              </Comp.Button>
              <Comp.Button
                variant="primary" 
                type='confirm'
                onClick={deleteBtnOnClick}
              >
                Confirm
              </Comp.Button>
            </Modal.Footer>
          </Modal.Body>
        </Modal>
      )}
    </>
  );
};

export default CustomerDelete;
