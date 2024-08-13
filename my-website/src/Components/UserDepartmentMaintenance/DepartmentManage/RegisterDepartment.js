import React, { useState, useRef } from 'react';
import { useCookies } from 'react-cookie';
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import 'react-toastify/dist/ReactToastify.min.css';
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { FcDepartment } from "react-icons/fc";
import { MdDescription } from "react-icons/md";
import { MdDomainAdd } from "react-icons/md";


const RegisterDepartment = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const departmentNameInputRef = useRef();
  const departmentDescInputRef = useRef();

  const [addData, setAddData] = useState({});
  const [inputError, setInputError] = useState({});

  const hideModal = () => {
    setAddData({});
    setInputError({});
    props.onHide();
  };

  const saveBtnOnClick = () => {
    let flag = true;
    if (!addData.DEPARTMENT_NAME) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        DEPARTMENT_NAME: "Department name could not be empty.",
      }));
    }

    if (flag) {
      RegisterNewDepartment();
    }
  };

  function RegisterNewDepartment() {
    let functionName = "";

    try {
      functionName = props.page + RegisterNewDepartment.name;
			props.onLoading(true, "Registering department, please wait...");

      const data = {
        DEPARTMENT_NAME: addData.DEPARTMENT_NAME.trim(),
        DEPARTMENT_DESC: addData.DEPARTMENT_DESC ? addData.DEPARTMENT_DESC.trim() : "",
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
      };

      http
        .post("api/department/InsertDepartment", data, { timeout: 5000 })
        .then((response) => {
          if (response.data.DUPLICATE_DEPARTMENT_NAME) {
            toast.error("Department name already exists.");
          } else {
            toast.success("Department is successfully inserted.");
            props.onReload();
            hideModal();
          }
        })
        .catch((err) => {
          toast.error("Failed to insert department. Please try again.");
          common.c_LogWebError(props.page, "RegisterNewDepartment", err);
        })
        .finally(() => {
          props.onLoading(false, "Loading...");
        });
    } catch (err) {
      toast.error("Failed to insert department. Please try again.");
      common.c_LogWebError(props.page, "RegisterNewDepartment", err);
    }
  };

  const inputRequiredHandler = (event) => {
    const name = event.target.name;
    const id = event.target.id;
    const val = event.target.value;
    if (val.length === 0 || val.trim().length === 0) {
      setInputError((prevState) => ({
        ...prevState,
        [id]: `${name} could not be empty.`,
      }));
    } else {
      setInputError((prevState) => ({
        ...prevState,
        [id]: "",
      }));
    }
    setAddData((prevState) => ({
      ...prevState,
      [id]: val,
    }));
  };

  const inputUnrequiredHandler = (event) => {
    const id = event.target.id;
    const val = event.target.value;
    setAddData((prevState) => ({
      ...prevState,
      [id]: val,
    }));
  };

  return (
    <Modal show={props.showHide} onHide={hideModal} centered>
      <Modal.Header closeButton>
        <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}> 
          <Modal.Title>ADD NEW DEPARTMENT &nbsp; <MdDomainAdd className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
        </div>
      </Modal.Header>
      <Modal.Body>
        <div>
        <FcDepartment className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
        <label label="Department Name">Department Name:</label>
        <Comp.Input
          ref={departmentNameInputRef}
          id="DEPARTMENT_NAME"
          name="Department Name"
          type="text"
          className="form-control"
          errorMessage={inputError.DEPARTMENT_NAME}
          value={addData.DEPARTMENT_NAME || ""}
          onChange={inputRequiredHandler}
        />
        </div>
        <br></br>
        <div>
        <MdDescription className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
        <label label="Description:">Description:</label>
        <Comp.Input
          ref={departmentDescInputRef}
          id="DEPARTMENT_DESC"
          type="text"
          className="form-control"
          value={addData.DEPARTMENT_DESC || ""}
          onChange={inputUnrequiredHandler}
        />
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
          &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
          <Comp.Button 
            id="btnSave" 
            onClick={saveBtnOnClick}
          >
            Save
          </Comp.Button>
        </div>
      </Modal.Footer>
    </Modal>
  );
};

export default RegisterDepartment;







