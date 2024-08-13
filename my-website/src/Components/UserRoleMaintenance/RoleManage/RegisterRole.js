import React, { useState, useRef } from 'react';
import { useCookies } from 'react-cookie';
import { toast } from "react-toastify";
import { Modal } from "react-bootstrap";
import 'react-toastify/dist/ReactToastify.min.css';
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { FaClipboardUser } from "react-icons/fa6";
import { MdDescription } from "react-icons/md";
import { TiUserAddOutline } from "react-icons/ti";

const RegisterRole = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const roleNameInputRef = useRef();
  const roleDescInputRef = useRef();

  const [addData, setAddData] = useState({});
  const [inputError, setInputError] = useState({});

  const hideModal = () => {
    setAddData({});
    setInputError({});
    props.onHide();
  };

  const saveBtnOnClick = () => {
    let flag = true;
    if (!addData.ROLE_NAME) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        ROLE_NAME: "Role name could not be empty.",
      }));
    }

    if (flag) {
      RegisterNewRole();
    }
  };

  const RegisterNewRole = () => {
    try {
      const data = {
        ROLE_NAME: addData.ROLE_NAME.trim(),
        ROLE_DESC: addData.ROLE_DESC ? addData.ROLE_DESC.trim() : "",
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
      };

      http
        .post("api/role/InsertRole", data, { timeout: 5000 })
        .then((response) => {
          if (response.data.DUPLICATE_ROLE_NAME) {
            toast.error("Role name already exists.");
          } else {
            toast.success("Role is successfully inserted.");
            props.onReload();
            hideModal();
          }
        })
        .catch((err) => {
          toast.error("Failed to insert role. Please try again.");
          common.c_LogWebError(props.page, "RegisterNewRole", err);
        })
        .finally(() => {
          props.onLoading(false, "Loading...");
        });
    } catch (err) {
      toast.error("Failed to insert role. Please try again.");
      common.c_LogWebError(props.page, "RegisterNewRole", err);
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
          <Modal.Title>ADD NEW ROLE &nbsp; <TiUserAddOutline className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
      </div>
      </Modal.Header>
      <Modal.Body>
        <div>
        <FaClipboardUser className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
        <label label="User Role">User Role:</label>
        <Comp.Input
          ref={roleNameInputRef}
          id="ROLE_NAME"
          name="User Role"
          type="text"
          className="form-control"
          errorMessage={inputError.ROLE_NAME}
          value={addData.ROLE_NAME || ""}
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
          value={addData.ROLE_DESC || ""}
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
            type="save"
            onClick={saveBtnOnClick}
          >
            Save
          </Comp.Button>
        </div>
      </Modal.Footer>
    </Modal>
  );
};

export default RegisterRole;
