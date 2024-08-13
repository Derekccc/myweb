import React, { useState, useEffect, useRef } from 'react';
import { Modal } from "react-bootstrap";
import { FaUserTie } from "react-icons/fa";
import { RiLockPasswordFill } from "react-icons/ri";
import { CgPassword } from "react-icons/cg";
import { BiSolidRename } from "react-icons/bi";
import { FaRegEye, FaEyeSlash } from "react-icons/fa";
import http from "../../Common/http-common";
import * as common from "../../Common/common";
import * as Comp from "../../Common/CommonComponents";

const LoginResetPassword = (props) => {
  const userIdInputRef = useRef();
  const userNameInputRef = useRef();
  const userPasswordInputRef = useRef();
  const userCPasswordInputRef = useRef();

  const [editData, setEditData] = useState({});
  const [inputError, setInputError] = useState([]);

  useEffect(() => {
    if (props.editData !== null) {
      setEditData({
        USER_ID: props.editData.USER_ID === null ? "" : props.editData.USER_ID,
        USER_NAME: props.editData.USER_NAME === null ? "" : props.editData.USER_NAME,
        ERROR_MSG: props.editData.ERROR_MSG === null ? "" : props.editData.ERROR_MSG,
      });
    }
    var inputText = document.getElementById("CONFIRM_PASSWORD");
    var saveBtn = document.getElementById("btnSave");
    if (inputText && saveBtn) {
      inputText.onkeydown = (e) => { if (e.key === "Enter") saveBtn.click(); }
    }
  }, [props.editData]);

  const hideModal = () => {
    setInputError([]);
    props.onHide();
  };

  const saveBtnOnClick = () => {
    if (editData.PASSWORD === undefined) {
        setInputError((prevState) => ({
          ...prevState,
          PASSWORD: ("Password could not be empty."),
        }));
      } else {
        furtherValidateInput("PASSWORD", editData.PASSWORD)
      }
      if (editData.CONFIRM_PASSWORD === undefined) {
        setInputError((prevState) => ({
          ...prevState,
          CONFIRM_PASSWORD: ("Confirm Password could not be empty."),
        }));
      } else {
        furtherValidateInput("CONFIRM_PASSWORD", editData.CONFIRM_PASSWORD)
      }
      if (Object.values(inputError).filter(v => v !== "").length === 0) {
        ChangePassword();
      }
    }
  
    function ChangePassword() {
      if (editData.PASSWORD !== undefined && editData.CONFIRM_PASSWORD !== undefined) {
        let functionName = "";
        try {
          functionName = ChangePassword.name;
        // props.onLoading(true, "Changing password, please wait...");
          var encrypted = common.c_EncryptData(editData.PASSWORD.trim());
          const data = {
            USER_ID: editData.USER_ID,
            USER_NAME: editData.USER_NAME,
            PASSWORD: encrypted.toString(),
            FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
          };
          http
            .put("api/login/ChangePassword", data, { timeout: 5000 })
            .then((response) => {
              if (response.data.VALID === "PASSWORD_REPEAT" || response.data.VALID === "PASSWORD_INVALID") {
                setEditData(prevState => ({
                  ...prevState,
                  ERROR_MSG: response.data.ERROR_MSG,
                }))
              }
              else {
                props.setCookies('USER_NAME', response.data.USER_NAME, { path: '/' });
                props.setCookies('USER_ID', response.data.USER_ID, { path: '/' });
                props.setCookies('AUTO_LOGOUT_DURATION', response.data.AUTO_LOGOUT_DURATION, { path: '/' });
                window.location = common.c_getWebUrl() + "Home";
                hideModal();
              }
            })
            .catch((err) => {
              common.c_LogWebError(props.page, functionName, err);
            })
            .finally(() => {
                console.log("Here is finally.")
            //   props.onLoading(false, "Loading...");
            });
        } catch (err) {
        //   props.onLoading(false, "Loading...");
          common.c_LogWebError(props.page, functionName, err);
        }
      }
    };
  

  function furtherValidateInput(id, val) {
    if (id === "CONFIRM_PASSWORD") {
      var PASSWORD = editData.PASSWORD;
      if (PASSWORD !== val) {
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
      var CPASSWORD = editData.CONFIRM_PASSWORD;
      if (CPASSWORD !== val) {
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
        <Modal.Header>
          <div style={{ color: 'yellow', backgroundColor: "black", width: "100%" }}>
            <Modal.Title>RESET PASSWORD</Modal.Title>
          </div>
        </Modal.Header>
        <Modal.Body>
            <div>
          <FaUserTie className='icon' style={{marginBottom: '10px', fontSize: '15px', marginRight: '10px'}}/>
          <label label="User Id">User ID:</label>
          <Comp.Input
            ref={userIdInputRef}
            id="USER_ID"
            type="text"
            placeholder="---"
            className="form-control"
            value={editData.USER_ID}
            readOnly={true}
          />
          </div>
          <div>
          <BiSolidRename className='icon' style={{ paddingRight: '10px', fontSize: '25px' }} />
          <label label="User Name">User Name:</label>
          <Comp.Input
            ref={userNameInputRef}
            id="USER_NAME"
            type="text"
            placeholder="---"
            className="form-control"
            value={editData.USER_NAME}
            readOnly={true}
          />
          </div>
          <RiLockPasswordFill className='icon' style={{ paddingRight: '10px', fontSize: '25px' }} />
          <label label="Password">Password*:</label>
          <Comp.Input
            ref={userPasswordInputRef}
            id="PASSWORD"
            name="Password"
            placeholder="Enter New Password"
            type="password"
            className="form-control"
            errorMessage={inputError.PASSWORD}
            value={editData.PASSWORD}
            onChange={inputRequiredHandler}
          />
          <CgPassword className='icon' style={{ paddingRight: '10px', fontSize: '25px' }} />
          <label label="Confirm Password">Confirm Password*:</label>
          <Comp.Input
            ref={userCPasswordInputRef}
            id="CONFIRM_PASSWORD"
            name="Confirm Password"
            placeholder="Confirm New Password"
            type="password"
            className="form-control"
            errorMessage={inputError.CONFIRM_PASSWORD}
            value={editData.CONFIRM_PASSWORD}
            onChange={inputRequiredHandler}
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
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
            <Comp.Button 
              id="btnSave" 
              onClick={saveBtnOnClick}
              >
              CONFIRM
            </Comp.Button>
            <p style={{ color: "rgb(255,0,0)", marginTop: "5px", fontSize: "smaller" }}>{editData.ERROR_MSG}</p>
          </div>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default LoginResetPassword;
