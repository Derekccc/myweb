import { Modal } from "react-bootstrap";
import * as Comp from "../../Common/CommonComponents";

const AlertModal = (props) => {
  return (
    <>
      <Modal show={props.show} onHide={props.onHide} centered>
        <Modal.Header>
          <div style={{ backgroundColor: "#e74c3c", color:"white", width: "100%" }}>
            <Modal.Title>ALERT</Modal.Title>
          </div>
        </Modal.Header>
        <div style={{ textAlign: "center" }}>
          <Modal.Body>{props.children}</Modal.Body>
        </div>
        <Modal.Footer>
          <Comp.Button id="btnCancel" type="cancel" onClick={props.onHide}>
            Cancel
          </Comp.Button>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default AlertModal;
