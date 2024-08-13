import { useState } from "react";
import { useCookies } from "react-cookie";
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { GrStatusGoodSmall } from "react-icons/gr";

const ProductActivator = (props) => {
	const [cookies] = useCookies([]);
	const [userId] = useState(cookies.USER_ID);
	
	//#region Modal Show/Hide
	const hideModal = () => {
		props.onHide();
	};
	//#endregion

	//#region Confirm Update Status
	const confirmBtnOnClick = () => {
		UpdateProductStatus(props.editData);
	};

	const UpdateProductStatus = (_data) => {
		let functionName = "";

		try {
			functionName = props.page + UpdateProductStatus.name;

			props.onLoading(
				true,
				(_data.STATUS === "Active" ? "Deactivating" : "Activating") +
				" product, please wait...",
			);
			props.onHide();
			const data = {
				PRODUCT_ID: _data.PRODUCT_ID,
				PRODUCT_NAME: _data.PRODUCT_NAME,
				PRODUCT_ACTIVE_FLAG: (_data.STATUS === "Active" ? "N" : "Y"),
				UPDATE_ID: userId,
				FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
			};
			const action = data.PRODUCT_ACTIVE_FLAG === "Y" ? "Activate" : "Deactivate";
			
			http
				.put("api/product/UpdateProductStatus", data, { timeout: 5000 })
				.then((response) => {
					toast.success("Product is successfully " + action + "d.");
					props.onReload();
				})
				.catch((err) => {
					toast.error("Failed to " + action + " Product. Please try again.");
					common.c_LogWebError(props.page, functionName, err);
				})
				.finally(() => {
					props.onLoading(false, "Loading...");
				});
		} catch (err) {
			props.onLoading(false, "Loading...");
			const action = _data.PRODUCT_ACTIVE_FLAG === "Y" ? "Deactivate" : "Activate";
			toast.error("Failed to " + action + " Product. Please try again.");
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
          
            <p style={{color: 'red'}}><b>Are you sure you want to "{props.editData !== null && props.editData.STATUS === "Active" ? "DEACTIVATE" : "ACTIVATE"}" product below ?</b></p>
            <hr/>
            <p><b>Product Name : {<b> {props.editData !== null && props.editData.PRODUCT_NAME}</b>}</b></p>
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

export default ProductActivator;