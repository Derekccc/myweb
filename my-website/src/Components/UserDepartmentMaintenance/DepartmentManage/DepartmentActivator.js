import { useState } from "react";
import { useCookies } from "react-cookie";
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { GrStatusGoodSmall } from "react-icons/gr";

const DepartmentActivator = (props) => {
	const [cookies] = useCookies([]);
	const [userId] = useState(cookies.USER_ID);
	
	//#region Modal Show/Hide
	const hideModal = () => {
		props.onHide();
	};
	//#endregion

	//#region Confirm Update Status
	const confirmBtnOnClick = () => {
		UpdateDepartmentStatus(props.editData);
	};

	const UpdateDepartmentStatus = (_data) => {
		let functionName = "";

		try {
			functionName = props.page + UpdateDepartmentStatus.name;

			props.onLoading(
				true,
				(_data.STATUS === "Active" ? "Deactivating" : "Activating") +
				" department, please wait...",
			);
			props.onHide();
			const data = {
				DEPARTMENT_ID: _data.DEPARTMENT_ID,
				DEPARTMENT_NAME: _data.DEPARTMENT_NAME,
				DEPARTMENT_ACTIVE_FLAG: (_data.STATUS === "Active" ? "N" : "Y"),
				UPDATE_ID: userId,
				FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
			};
			const action = data.DEPARTMENT_ACTIVE_FLAG === "Y" ? "Activate" : "Deactivate";
			
			http
				.put("api/department/UpdateDepartmentStatus", data, { timeout: 5000 })
				.then((response) => {
					toast.success("Department is successfully " + action + "d.");
					props.onReload();
				})
				.catch((err) => {
					toast.error("Failed to " + action + " Department. Please try again.1");
					common.c_LogWebError(props.page, functionName, err);
				})
				.finally(() => {
					props.onLoading(false, "Loading...");
				});
		} catch (err) {
			props.onLoading(false, "Loading...");
			const action = _data.DEPARTMENT_ACTIVE_FLAG === "Y" ? "deactivate" : "activate";
			toast.error("Failed to " + action + " Department. Please try again.");
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
          
            <p style={{color: 'red'}}><b>Are you sure you want to "{props.editData !== null && props.editData.STATUS === "Active" ? "DEACTIVATE" : "ACTIVATE"}" department below ?</b></p>
            <hr/>
            <p><b>Department Name : {<b> {props.editData !== null && props.editData.DEPARTMENT_NAME}</b>}</b></p>
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

export default DepartmentActivator;