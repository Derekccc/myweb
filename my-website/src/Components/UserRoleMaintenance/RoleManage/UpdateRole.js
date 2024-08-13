import { useEffect, useRef, useState } from "react";
import { useCookies } from "react-cookie";
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { FaClipboardUser } from "react-icons/fa6";
import { MdDescription } from "react-icons/md";
import { GrUpdate } from "react-icons/gr";


const UpdateRole = (props) => {
  //#region React Hooks
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const roleNameInputRef = useRef();
  const roleDescInputRef = useRef();
  
  const [editData, setEditData] = useState({});
  const [inputError, setInputError] = useState({});

  useEffect(() => {
    setEditData({
      ROLE_NAME: props.editData ? (props.editData.ROLE_NAME || "") : "",
      ROLE_DESC: props.editData ? (props.editData.ROLE_DESC || "") : "",
      ROLE_ID: props.editData ? (props.editData.ROLE_ID || "") : "",
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
      SaveEditRole();
    }
  };

  function SaveEditRole() {
    let functionName = "";

    try {
      functionName = props.page + SaveEditRole.name;

      props.onLoading(true, "Modifying role, please wait...");

      const data = {
        ROLE_ID: editData.ROLE_ID,
        ROLE_NAME: editData.ROLE_NAME.trim(),
        ROLE_DESC: editData.ROLE_DESC.trim(),
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
      };

      http
        .put("api/role/UpdateRole", data, { timeout: 10000 })
        .then((response) => {
          if (response.data.DUPLICATE_ROLE_NAME) {
            toast.error("Role name already exists.");
          } else {
            toast.success("Role successfully updated.");
            props.onReload();
            hideModal();
          }
        })
        .catch((err) => {
          toast.error("Failed to update role. Please try again.");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading...");
        });
    } catch (err) {
      props.onLoading(false, "Loading...");
      toast.error("Failed to update role. Please try again.");
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
          <FaClipboardUser className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Role Name">Role Name:</label>
          <Comp.Input
            ref={roleNameInputRef}
            id="ROLE_NAME"
            name="Role Name"
            type="text"
            className="form-control"
            errorMessage={inputError.ROLE_NAME}
            value={editData.ROLE_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <br></br>
          <div>
          <MdDescription className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Description">Description:</label>
          <Comp.Input
            ref={roleDescInputRef}
            id="ROLE_DESC"
            type="text"
            className="form-control"
            value={editData.ROLE_DESC}
            onChange={inputUnrequiredHandler}
          />
          <br></br>
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

export default UpdateRole;
