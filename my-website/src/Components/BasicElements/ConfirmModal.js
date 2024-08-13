import { Modal } from "react-bootstrap";
import * as Comp from "../../Common/CommonComponents";

const ConfirmModal = (props) => {
  let buttonYes = "";
  let buttonNo = "";

  if (props.delete) {
    buttonYes = (
      <Comp.Button type="delete" onClick={props.onConfirm}>
        Yes
      </Comp.Button>
    );
    buttonNo = (
      <Comp.Button id="btnCancel" type="cancel" onClick={props.onHide}>
        No
      </Comp.Button>
    );
  } else if (props.confirm) {
    buttonYes = (
      <Comp.Button type="delete" onClick={props.onConfirm} > 
        YES
      </Comp.Button>
    );
    buttonNo = (
      <Comp.Button id="btnCancel" type="cancel" onClick={props.onHide}>
        NO
      </Comp.Button>
    );
  } else {
    buttonYes = (
      <Comp.Button type="general" onClick={props.onConfirm}>
        Save
      </Comp.Button>
    );
    buttonNo = (
      <Comp.Button id="btnCancel" type="cancel" onClick={props.onHide}>
        Cancel
      </Comp.Button>
    );
  }

  return (
    <>
      <Modal show={props.show} onHide={props.onHide} centered>
        <Modal.Header>
          <div style={{ backgroundColor: "lightgrey", width: "100%" }}>
            <Modal.Title>CONFIRMATION</Modal.Title>
          </div>
        </Modal.Header>
        <div style={{ textAlign: "center" }}>
          <Modal.Body>{props.children}</Modal.Body>
        </div>
        <Modal.Footer>
          <div style={{ textAlign: "center", width: "100%" }}>
            {buttonYes}
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
            {buttonNo}
          </div>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default ConfirmModal;
