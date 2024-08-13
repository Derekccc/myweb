import { useState } from "react";
import { useCookies } from "react-cookie";
import { toast } from "react-toastify";
import { Modal } from 'react-bootstrap';
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { FcUnlock } from "react-icons/fc";

const CustomerUnlock = (props) => {
	const [cookies] = useCookies([]);
	const [userId] = useState(cookies.USER_ID);
	
	//#region Modal Show/Hide
	const hideModal = () => {
		props.onHide();
	};
	//#endregion

    // const handleCancel = () => {
    //     setConfirmDelete(false);
    //     props.onHide();
    //   };

	//#region Confirm Update Status
	const confirmBtnOnClick = () => {
		UnlockCustomer(props.editData);
	};

	const UnlockCustomer = (_data) => {
		let functionName = "";

		try {
			functionName = props.page + UnlockCustomer.name;

			props.onLoading(true, "Unlocking customer, please wait...");
			props.onHide();
			const data = {
				CUSTOMER_ID: _data.CUSTOMER_ID,
				CUSTOMER_NAME: _data.CUSTOMER_NAME,
				UPDATE_ID: userId,
				FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
			};			
			http
				.put("api/customer/UnlockCustomer", data, { timeout: 5000 })
				.then((response) => {
					toast.success("Customer is successfully unlocked.");
					props.onReload();
				})
				.catch((err) => {
					toast.error("Failed to unlock customer. Please try again.1");
					common.c_LogWebError(props.page, functionName, err);
				})
				.finally(() => {
					props.onLoading(false, "Loading...");
				});
		} catch (err) {
			props.onLoading(false, "Loading...");
			toast.error("Failed to unlock customer. Please try again.");
			common.c_LogWebError(props.page, functionName, err);
		}
	};
	//#endregion

	return (
		<>
	    <Modal show={props.showHide} onHide={hideModal}>
          <Modal.Header closeButton>
          <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>        
          <Modal.Title>Confirm Unlock &nbsp; <FcUnlock className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
          </div>
          </Modal.Header>
          <Modal.Body>
          
            <p style={{color: 'red'}}><b>Are you sure you want to "UNLOCK" customer below ?</b></p>
            <hr/>
            <p><b>Customer ID : {<b> {props.editData !== null && props.editData.CUSTOMER_ID}</b>}</b></p>
          </Modal.Body>
          <Modal.Footer>
            <Comp.Button 
              variant="secondary" 
              type='cancel'
              onClick={hideModal}
            >
              Cancel
            </Comp.Button>
            <Comp.Button 
              variant="primary" 
              type='confirm'
              onClick={confirmBtnOnClick}
            >
              Confirm
            </Comp.Button>
          </Modal.Footer>
        </Modal>
		</>
	);
};

export default CustomerUnlock;