import { useEffect, useRef, useState } from "react";
import { useCookies } from "react-cookie";
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { FcDepartment } from "react-icons/fc";
import { MdDescription } from "react-icons/md";
import { GrUpdate } from "react-icons/gr";


const UpdateDepartment = (props) => {
  //#region React Hooks
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const departmentNameInputRef = useRef();
  const departmentDescInputRef = useRef();

  const [editData, setEditData] = useState([]);

  const [inputError, setInputError] = useState([]);

  useEffect(() => {
    setEditData({
      DEPARTMENT_NAME: props.editData !== null ? (props.editData.DEPARTMENT_NAME || "") : "",
      DEPARTMENT_DESC: props.editData !== null ? (props.editData.DEPARTMENT_DESC || "") : "",
      DEPARTMENT_ID: props.editData !== null ? (props.editData.DEPARTMENT_ID || "") : "",
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
      SaveEditDepartment();
    }
  };

  function SaveEditDepartment() {
    let functionName = "";

    try {
      functionName = props.page + SaveEditDepartment.name;

      props.onLoading(true, "Modifying department, please wait...");

      const data = {
        DEPARTMENT_ID: editData.DEPARTMENT_ID,
        DEPARTMENT_NAME: editData.DEPARTMENT_NAME.trim(),
        DEPARTMENT_DESC: editData.DEPARTMENT_DESC.trim(),
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
      };

      http
        .put("api/department/UpdateDepartment", data, { timeout: 10000 })
        .then((response) => {
          if (response.data.DUPLICATE_DEPARTMENT_NAME) {
            toast.error("Department name already exists.");
          } else {
            toast.success("Department successfully updated.");
            props.onReload();
            hideModal();
          }
        })
        .catch((err) => {
          toast.error("Failed to update department. Please try again.");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading...");
        });
    } catch (err) {
      props.onLoading(false, "Loading...");
      toast.error("Failed to update department. Please try again.");
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
          <FcDepartment className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Department Name:">Department Name:</label>
          <Comp.Input
            ref={departmentNameInputRef}
            id="DEPARTMENT_NAME"
            name="Department Name"
            type="text"
            className="form-control"
            errorMessage={inputError.DEPARTMENT_NAME}
            value={editData.DEPARTMENT_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <br></br>
          <MdDescription className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Decription:">Decription:</label>
          <Comp.Input
            ref={departmentDescInputRef}
            id="DEPARTMENT_DESC"
            type="text"
            className="form-control"
            value={editData.DEPARTMENT_DESC}
            onChange={inputUnrequiredHandler}
          />
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

export default UpdateDepartment;