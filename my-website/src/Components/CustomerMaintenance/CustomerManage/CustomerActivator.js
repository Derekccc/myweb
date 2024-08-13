import { useState } from "react";
import { useCookies } from "react-cookie";
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { GrStatusGoodSmall } from "react-icons/gr";

const UserActivator = (props) => {
	const [cookies] = useCookies([]);
	const [userId] = useState(cookies.USER_ID);
	
	//#region Modal Show/Hide
	const hideModal = () => {
		props.onHide();
	};
	//#endregion

	//#region Confirm Update Status
	const confirmBtnOnClick = () => {
		UpdateCustomerStatus(props.editData);
	};

	const UpdateCustomerStatus = (_data) => {
		let functionName = "";

		try {
			functionName = props.page + UpdateCustomerStatus.name;

			props.onLoading(
				true,
				(_data.STATUS === "Active" ? "Deactivating" : "Activating") +
				" customer, please wait...",
			);
			props.onHide();
			const data = {
				CUSTOMER_ID: _data.CUSTOMER_ID,
				CUSTOMER_NAME: _data.CUSTOMER_NAME,
				ACTIVE_FLAG: (_data.STATUS === "Active" ? "N" : "Y"),
				UPDATE_ID: userId,
				FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
			};
			const action = data.ACTIVE_FLAG === "Y" ? "Activate" : "Deactivate";
			
			http
				.put("api/customer/UpdateCustomerStatus", data, { timeout: 5000 })
				.then((response) => {
					toast.success("Customer is successfully " + action + "d.");
					props.onReload();
				})
				.catch((err) => {
					toast.error("Failed to " + action + " Customer. Please try again.");
					common.c_LogWebError(props.page, functionName, err);
				})
				.finally(() => {
					props.onLoading(false, "Loading...");
				});
		} catch (err) {
			props.onLoading(false, "Loading...");
			const action = _data.ACTIVE_FLAG === "Y" ? "Deactivate" : "Activate";
			toast.error("Failed to " + action + " Customer. Please try again.");
			common.c_LogWebError(props.page, functionName, err);
		}
	};
	//#endregion

    const iconColor = props.editData !== null && props.editData.STATUS === "Active" ? "red" : "green"

	return (
		<>
			<Modal show={props.showHide} onHide={hideModal}>
          <Modal.Header closeButton>
          <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>        
          <Modal.Title>Confirm {props.editData !== null && props.editData.STATUS === "Active" ? "Deactivate" : "Activate"} ? &nbsp; 
            <GrStatusGoodSmall className='icon' style={{ fontSize: '30px', color: iconColor, marginBottom: '5px'}}/></Modal.Title>
          </div>
          </Modal.Header>
          <Modal.Body>
          
            <p style={{color: 'red'}}><b>Are you sure you want to "{props.editData !== null && props.editData.STATUS === "Active" ? "DEACTIVATE" : "ACTIVATE"}" user below ?</b></p>
            <hr/>
            <p><b>CUSTOMER NAME : {<b> {props.editData !== null && props.editData.CUSTOMER_NAME}</b>}</b></p>
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

export default UserActivator;