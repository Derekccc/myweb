import React, { useEffect, useState, useRef } from 'react';
import { useCookies } from 'react-cookie';
import { Modal } from "react-bootstrap";
import { toast } from 'react-toastify';
import http from '../../../Common/http-common';
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { PiPasswordDuotone } from "react-icons/pi";
import { BiSolidUserPin } from "react-icons/bi";
import { BiSolidRename } from "react-icons/bi";
import { RiLockPasswordFill } from "react-icons/ri";
import { CgPassword } from "react-icons/cg";


const UserResetPassword = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);

  const userIdInputRef = useRef();
  const userNameInputRef = useRef();
  const userPasswordInputRef = useRef();
  const userCPasswordInputRef = useRef();

  const [editData, setEditData] = useState([]);
  const [inputError, setInputError] = useState([]);

  useEffect(() => {
    if (props.editData !== null) {
      setEditData({
        USER_ID: props.editData.USER_ID === null ? "" : props.editData.USER_ID,
        USER_NAME: props.editData.USER_NAME === null ? "" : props.editData.USER_NAME,
      });
    }
  }, [props.editData]);

    const hideModal = () => {
      setInputError([]);
      props.onHide();
    };

    const saveBtnOnClick = () => {
      if (editData.DEFAULT_PASSWORD === undefined) {
        setInputError((prevState) => ({
          ...prevState,
          DEFAULT_PASSWORD: (" Password could not be empty."),
        }));
      } else {
        furtherValidateInput("DEFAULT_PASSWORD", editData.DEFAULT_PASSWORD)
      }
      if (Object.values(inputError).filter(v => v !== "").length === 0) {
        SaveEditUser();
      }
    }

    function SaveEditUser() {
      let functionName = "";
      try {
        functionName = SaveEditUser.name;
        props.onLoading(true, "Modifying user, please wait...");
        var encrypted = common.c_EncryptData(editData.DEFAULT_PASSWORD.trim());
        const data = {
          USER_ID: editData.USER_ID,
          USER_NAME: editData.USER_NAME,
          DEFAULT_PASSWORD: encrypted.toString(),
          UPDATE_ID: userId,
          FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
        };
        http
          .put("api/user/ResetPassword", data, { timeout: 5000 })
          .then((response) => {
            toast.success("User password is successfully reset.");
            props.onReload();
            hideModal();
          })
          .catch((err) => {
            toast.error("Failed to reset user password. Please try again.1");
            common.c_LogWebError(props.page, functionName, err);
          })
          .finally(() => {
            props.onLoading(false, "Loading...");
          });
      } catch (err) {
        props.onLoading(false, "Loading...");
        toast.error("Failed to reset user password. Please try again.2");
        common.c_LogWebError(props.page, functionName, err);
      }
    };

    function furtherValidateInput(id, val) {
      if (id === "DEFAULT_PASSWORD") {
        var CONFIRM_PASSWORD = editData.CONFIRM_PASSWORD;
        if (CONFIRM_PASSWORD !== val) {
          setInputError((prevState) => ({
            ...prevState,
            CONFIRM_PASSWORD: ("Please confirm the passwords are match."),
          }));
        } else {
          setInputError((prevState) => ({
            ...prevState,
            CONFIRM_PASSWORD: (""),
          }));
        }
      } else {
        var DEFAULT_PASSWORD = editData.DEFAULT_PASSWORD;
        if (DEFAULT_PASSWORD !== val) {
          setInputError((prevState) => ({
            ...prevState,
            CONFIRM_PASSWORD: ("Please confirm the passwords are match."),
          }));
        } else {
          setInputError((prevState) => ({
            ...prevState,
            CONFIRM_PASSWORD: (""),
          }));
        }
      }
    }

    const inputRequiredHandler = (event) => {
      let functionName = "";
  
      try {
        functionName = inputRequiredHandler.name;
        const name = event.target.name;
        functionName = inputRequiredHandler.name + "_" + name;
        const id = event.target.id;
        const val = event.target.value;
        if (val.length === 0 || val.trim().length === 0) {
          setInputError((prevState) => ({
            ...prevState,
            [id]: (name + " could not be empty."),
          }));
        } else {
          setInputError((prevState) => ({
            ...prevState,
            [id]: (""),
          }));
          furtherValidateInput(id, val);
        }
        setEditData((prevState) => ({
          ...prevState,
          [id]: val,
        }));
      } catch (err) {
        common.c_LogWebError(props.page, functionName, err);
      }
    }

    return (
      <>
        <Modal show={props.showHide} onHide={hideModal} centered>
          <Modal.Header closeButton>
            <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>
              <Modal.Title>RESET PASSWORD &nbsp; <PiPasswordDuotone className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
            </div>
          </Modal.Header>
          <Modal.Body>
            <div>
            <BiSolidUserPin className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
            <label label="User ID">User ID :</label>
            <Comp.Input
              ref={userIdInputRef}
              id="USER_ID"
              type="text"
              className="form-control"
              value={editData.USER_ID}
              readOnly={true}
            />
            </div>
            <div>
            <BiSolidRename className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
            <label label="User Name">User Name :</label>
            <Comp.Input
              ref={userNameInputRef}
              id="USER_NAME"
              type="text"
              className="form-control"
              value={editData.USER_NAME}
              readOnly={true}
            />
            </div>
            <div>
            <RiLockPasswordFill className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
            <label label="Password">Password :</label>
            <Comp.Input
              ref={userPasswordInputRef}
              id="DEFAULT_PASSWORD"
              name="Password"
              placeholder= "Enter New Password"
              type="password"
              className="form-control"
              errorMessage={inputError.DEFAULT_PASSWORD}
              value={editData.DEFAULT_PASSWORD}
              onChange={inputRequiredHandler}
            />
            </div>
            <div>
            <CgPassword className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
            <label label="Confirm Password">Confirm Password :</label>
            <Comp.Input
              ref={userCPasswordInputRef}
              id="CONFIRM_PASSWORD"
              name="Confirm Password"
              placeholder= "Confirm New Password"
              type="password"
              className="form-control"
              errorMessage={inputError.CONFIRM_PASSWORD}
              value={editData.CONFIRM_PASSWORD}
              onChange={inputRequiredHandler}
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
      </>
    );
};

export default UserResetPassword;

