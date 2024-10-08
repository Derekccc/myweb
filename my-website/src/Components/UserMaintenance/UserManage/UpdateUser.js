import React, { useState, useEffect, useRef } from 'react';
import { toast } from 'react-toastify';
import { useCookies } from 'react-cookie';
import http from '../../../Common/http-common';
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { Modal } from "react-bootstrap";
import { GrUpdate } from "react-icons/gr";
import { BiSolidUserPin, BiSolidRename } from "react-icons/bi";
import { AiTwotoneMail } from "react-icons/ai";
import { FaUserAstronaut } from "react-icons/fa";
import { FcDepartment } from "react-icons/fc";




const UpdateUser = (props) => {
  //#region React Hooks
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const userIdInputRef = useRef();
  const userNameInputRef = useRef();
  const userEmailInputRef = useRef();

  const [roleListToBeSelect, setRoleListToBeSelect] = useState([]);
  const [roleList, setRoleList] = useState([]);
  const [departmentListToBeSelect, setDepartmentListToBeSelect] = useState([]);
  const [departmentList, setDepartmentList] = useState([]);

  const [editData, setEditData] = useState([]);
  const [inputError, setInputError] = useState([]);

  useEffect(() => {
    if (props.editData !== null) {
      setEditData({
        USER_ID: props.editData.USER_ID === null ? "" : props.editData.USER_ID,
        USER_NAME: props.editData.USER_NAME === null ? "" : props.editData.USER_NAME,
        EMAIL: props.editData.EMAIL === null ? "" : props.editData.EMAIL,

        ROLE_ID: props.editData.ROLE_ID === null ? "" : props.editData.ROLE_ID,
        ROLE_NAME: props.editData.ROLE_NAME === null ? "" : props.editData.ROLE_NAME,
        DEPARTMENT_ID: props.editData.DEPARTMENT_ID === null ? "" : props.editData.DEPARTMENT_ID,
        DEPARTMENT_NAME: props.editData.DEPARTMENT_NAME === null ? "" : props.editData.DEPARTMENT_NAME,
      })
      // for select role dropdown
      const roleId = props.editData.ROLE_ID === null || props.editData.ROLE_ID === undefined ? "" : props.editData.ROLE_ID.split(",");
      const roleName = props.editData.ROLE_NAME === null || props.editData.ROLE_NAME === undefined ? "" : props.editData.ROLE_NAME.split(",");
      const role = [];
      for (var i = 0; i < roleId.length; i++) {
        role.push({
          key: roleId[i],
          value: roleId[i],
          label: roleName[i],
        });
      }
      setRoleList(role);

      // for select department dropdown
      const departmentId = props.editData.DEPARTMENT_ID === null || props.editData.DEPARTMENT_ID === undefined ? "" : props.editData.DEPARTMENT_ID.split(",");
      const departmentName = props.editData.DEPARTMENT_NAME === null || props.editData.DEPARTMENT_NAME === undefined ? "" : props.editData.DEPARTMENT_NAME.split(",");
      const department = [];
      for (var i = 0; i < departmentId.length; i++) {
        department.push({
          key: departmentId[i],
          value: departmentId[i],
          label: departmentName[i],
        });
      }
      setDepartmentList(department);
    }
  }, [props.editData]);
  //endregion

  //#region Modal Show/Hide
  const hideModal = () => {
    setInputError([]);
    props.onHide();
  };
  //#endregion

  const saveBtnOnClick = () => {
    if (Object.values(inputError).filter(v => v !== "").length === 0){
      saveEditUser();
    }
  }

  //#region Save Edit
  function saveEditUser() {
    let functionName = "";
    try {
      functionName = saveEditUser.name;
      props.onLoading(true, "Updating user, Please wait....");
      const selectedRoleList = roleList.map(r => r.key);
      const selectedDepartmentList = departmentList.length > 0 ? [departmentList[0].key] : [];
      const data = {
        USER_ID: editData.USER_ID.trim(),
        USER_NAME: editData.USER_NAME.trim(),
        EMAIL: editData.EMAIL !== undefined ? editData.EMAIL.trim() : "",
        FROM_SOURCE: {SOURCE: "WEB", MODULE_ID: props.module },
        UPDATE_ID: userId,
        USER_ROLE_LIST: selectedRoleList,
        USER_DEPARTMENT_LIST: selectedDepartmentList,
      };
      http
        .put("api/user/UpdateUser", data, { timeout: 10000})
        .then((response) => {
          toast.success("User is successfully Updated.");
          props.onReload();
          hideModal();
        })
        .catch((err) => {
          toast.error("Failed to update user. Please try again.....(send)");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading....");
        });
    } catch (err) {
      props.onLoading(false, "Loading...");
      toast.error("Failed to update user. Please try again.....(receive)");
      common.c_LogWebError(props.page, functionName, err);
    }
  };
  //#endregion

  //#region useEffect to get role list and department list
  useEffect(() => {
    let functionName = "";

    try {
      functionName = "Get Role Name";
      http
        .get(
          "api/user/GetRoleList?_sysUser=" + userId
        )
        .then((response) => {
          const rolelist = [];
          for (var i = 0; i < response.data.length; i++) {
            rolelist.push({
              key: response.data[i].ROLE_ID,
              value: response.data[i].ROLE_ID,
              label: response.data[i].ROLE_NAME,
            });
          }
          setRoleListToBeSelect(rolelist);
        })
        .catch((err) => {
          common.c_LogWebError(props.page, functionName, err);
        });
    } catch (err) {
      common.c_LogWebError(props.page, functionName, err);
    }
  }, []);

  useEffect(() => {
    let functionName = "";

    try {
      functionName = "Get Department Name";
      http
        .get(
          "api/user/GetDepartmentList?_sysUser=" + userId
        )
        .then((response) => {
          const departmentlist = [];
          for (var i = 0; i < response.data.length; i++) {
            departmentlist.push({
              key: response.data[i].DEPARTMENT_ID,
              value: response.data[i].DEPARTMENT_ID,
              label: response.data[i].DEPARTMENT_NAME,
            });
          }
          setDepartmentListToBeSelect(departmentlist);
        })
        .catch((err) => {
          common.c_LogWebError(props.page, functionName, err);
        });
    } catch (err) {
      common.c_LogWebError(props.page, functionName, err);
    }
  }, []);
  //#endregion

  function furtherValidateInput(id, val) {
    if (id === "EMAIL") {
      if (!common.EMAIL_REGEX.test(val) && val !== "") {
        setInputError((prevState) => ({
          ...prevState,
          [id]: ("Please enter a proper email."),
        }));
      } else {
        setInputError((prevState) => ({
          ...prevState,
          [id]: (""),
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

  const inputUnrequiredHandler = (event) => {
    let functionName = "";

    try {
      functionName = inputUnrequiredHandler.name;
      const id = event.target.id;
      const val = event.target.value;

      furtherValidateInput(id, val);
      setEditData((prevState) => ({
        ...prevState,
        [id]: val,
      }));
    } catch (err) {
      common.c_LogWebError(props.page, functionName, err);
    }
  }

  const selectUserRoleHandler = (val) => {
    let functionName = "";

    try {
      functionName = selectUserRoleHandler.name;
      if (Object.keys(val).length > 0) {
        setInputError((prevState) => ({
          ...prevState,
          USER_ROLE: (""),
        }));
      } else {
        setInputError((prevState) => ({
          ...prevState,
          USER_ROLE: ("User role could not be empty or blank. Thank you."),
        }));
      }
      setRoleList([...val]);
    } catch (err) {
      common.c_LogWebError(props.page, functionName, err);
    }
  }

  const selectUserDepartmentHandler = (val) => {
    let functionName = "";

    try {
      functionName = selectUserDepartmentHandler.name;
      if (val) {
        setInputError((prevState) => ({
          ...prevState,
          USER_DEPARTMENT: (""),
        }));
      } else {
        setInputError((prevState) => ({
          ...prevState,
          USER_DEPARTMENT: ("User department could not be empty. Thank you."),
        }));
      }
      setDepartmentList([val]);
    } catch (err) {
      common.c_LogWebError(props.page, functionName, err);
    }
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
            name="User Name"
            type="text"
            className="form-control"
            errorMessage={inputError.USER_NAME}
            value={editData.USER_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <AiTwotoneMail className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Email">Email :</label>
          <Comp.Input
            ref={userEmailInputRef}
            id="EMAIL"
            name="Email"
            type="text"
            className="form-control"
            errorMessage={inputError.EMAIL}
            value={editData.EMAIL}
            onChange={inputUnrequiredHandler}
          />
          </div>
          <div>
          <FaUserAstronaut className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="User Role">User Role :</label>
          <Comp.Select
            id="USER_ROLE"
            type="text"
            options={roleListToBeSelect}
            closeMenuOnSelect={true}
            hideSelectedOptions={false}
            errorMessage={inputError.USER_ROLE}
            value={roleList}
            onChange={selectUserRoleHandler}
            isMulti
          />
          </div>
          <br></br>
          <div>
          <FcDepartment className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="User Department">User Department :</label>
          <Comp.Select
            id="USER_DEPARTMENT"
            type="text"
            options={departmentListToBeSelect}
            closeMenuOnSelect={true}
            hideSelectedOptions={false}
            errorMessage={inputError.USER_DEPARTMENT}
            value={departmentList}
            onChange={selectUserDepartmentHandler}
            isMulti = {false}
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

export default UpdateUser;
