import { useState } from "react";
import { useCookies } from "react-cookie";
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { GrStatusGoodSmall } from "react-icons/gr";

const SalesOrderReviewer = (props) => {
	const [cookies] = useCookies([]);
	const [userId] = useState(cookies.USER_ID);
	
	//#region Modal Show/Hide
	const hideModal = () => {
		props.onHide();
	};
	//#endregion

	//#region Confirm Update Status
	const confirmBtnOnClick = () => {
		UpdateOrderReview(props.editData);
	};

	const UpdateOrderReview = (_data) => {
		let functionName = "";

		try {
			functionName = props.page + UpdateOrderReview.name;

			props.onLoading(
				true,
				(_data.REVIEW === "Accept" ? "Rejecting" : "Accepting") +
				" sales order, please wait...",
			);
			props.onHide();
			const data = {
				SALES_ORDER_ID: _data.SALES_ORDER_ID,
				SALES_ORDER_ACCEPT_FLAG: (_data.REVIEW === "Accept" ? "N" : "Y"),
				UPDATE_ID: userId,
				FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
			};
			const action = data.SALES_ORDER_ACCEPT_FLAG === "Y" ? "Accept" : "Reject";
			
			http
				.put("api/salesOrder/UpdateOrderStatus", data, { timeout: 5000 })
				.then((response) => {
					toast.success("Sales order is successfully " + action + "ed.");
					props.onReload();
				})
				.catch((err) => {
					toast.error("Failed to " + action + " Sales Order. Please try again.");
					common.c_LogWebError(props.page, functionName, err);
				})
				.finally(() => {
					props.onLoading(false, "Loading...");
				});
		} catch (err) {
			props.onLoading(false, "Loading...");
			const action = _data.SALES_ORDER_ACCEPT_FLAG === "Y" ? "Reject" : "Accept";
			toast.error("Failed to " + action + " Sales Order. Please try again.");
			common.c_LogWebError(props.page, functionName, err);
		}
	};
	//#endregion

    const iconColor = props.editData !== null && props.editData.REVIEW === "Accept" ? "red" : "green"

	return (
		<>
			<Modal show={props.showHide} onHide={hideModal}>
          <Modal.Header closeButton>
          <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>        
          <Modal.Title>Confirm {props.editData !== null && props.editData.REVIEW === "Accept" ? "Reject" : "Accept"} ? &nbsp; 
            <GrStatusGoodSmall className='icon' style={{ fontSize: '30px', color: iconColor, marginBottom: '5px'}}/></Modal.Title>
          </div>
          </Modal.Header>
          <Modal.Body>
          
            <p style={{color: 'red'}}><b>Are you sure you want to "{props.editData !== null && props.editData.REVIEW === "Accept" ? "REJECT" : "ACCEPT"}" sales order below ?</b></p>
            <hr/>
            <p><b>Sales Order ID : {<b> {props.editData !== null && props.editData.SALES_ORDER_ID}</b>}</b></p>
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

export default SalesOrderReviewer;