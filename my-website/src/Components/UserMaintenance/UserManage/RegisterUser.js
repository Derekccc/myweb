import { useState, useEffect, useRef } from 'react';
import { useCookies } from 'react-cookie';
import { toast } from "react-toastify";
import { Modal } from 'react-bootstrap';
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { GiArchiveRegister } from "react-icons/gi";
import { IoIdCard } from "react-icons/io5";
import { SiNamemc } from "react-icons/si";
import { MdEmail } from "react-icons/md";
import { FaUserAstronaut } from "react-icons/fa";
import { FcDepartment  } from "react-icons/fc";
import { RiLockPasswordFill } from "react-icons/ri";
import { GiConfirmed } from "react-icons/gi";


const RegisterUser = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);

  const userIdInputRef = useRef();
  const userNameInputRef = useRef();
  const userEmailInputRef = useRef();
  const userPasswordInputRef = useRef();
  const userCPasswordInputRef = useRef();

  const [roleListToBeSelect, setRoleListToBeSelect] = useState([]);
  const [roleList, setRoleList] = useState([]);

  const [departmentListToBeSelect, setDepartmentListToBeSelect] = useState([]);
  const [departmentList, setDepartmentList] = useState([]);

  const [addData, setAddData] = useState([]);
  const [inputError, setInputError] = useState([]);

  const hideModal = () => {
    setRoleList([]);
    setAddData([]);
    setInputError([]);
    props.onHide();
  };

  //#region Function Onlick
  const saveBtnOnClick = () => {
    var flag = true;
    if (addData.USER_ID === undefined) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        USER_ID: (" User ID could not be empty."),
      }));
    }
    if (addData.USER_NAME === undefined) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        USER_NAME: (" User Name could not be empty."),
      }));
    }

    if (addData.EMAIL !== undefined) {
      flag = furtherValidateInput("EMAIL", addData.EMAIL) && flag;
    }
    if (Object.keys(roleList).length === 0) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        USER_ROLE: ("User role could not be empty."),
      }));
    } else {
      setInputError((prevState) => ({
        ...prevState,
        USER_ROLE: (""),
      }));
    }
    if (Object.keys(departmentList).length === 0) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        USER_DEPARTMENT: ("User department could not be empty."),
      }));
    } else {
      setInputError((prevState) => ({
        ...prevState,
        USER_DEPARTMENT: (""),
      }));
    }
    if (addData.DEFAULT_PASSWORD === undefined) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        DEFAULT_PASSWORD: "Password could not be empty.",
      }));
    } else {
      flag = furtherValidateInput("DEFAULT_PASSWORD", addData.DEFAULT_PASSWORD)
    }
    if (Object.values(inputError).filter(v => v !== "").length === 0 && flag) {
      RegisterNewUser();
    }
  }

  function RegisterNewUser() {
    let functionName = "";
    try {
      functionName = props.page + RegisterNewUser.name;
      props.onLoading(true, "Registering user, please wait...");
      var encrypted = common.c_EncryptData(addData.DEFAULT_PASSWORD.trim());
      const selectedRoleList = roleList.map(r => r.key);
      const selectedDepartmentList = departmentList.length > 0 ? [departmentList[0].key] : [];

      const data = {
        USER_ID: addData.USER_ID.trim(),
        USER_NAME: addData.USER_NAME.trim(),
        EMAIL: addData.EMAIL !== undefined ? addData.EMAIL.trim() : "",
        // USERROLE_ID: addData.USER_CATEGORY.key,
        DEFAULT_PASSWORD: encrypted.toString(),
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
        USER_ROLE_LIST: selectedRoleList,
        USER_DEPARTMENT_LIST: selectedDepartmentList,
      };
      http
        .post("api/user/RegisterUser", data, { timeout: 10000 })
        .then((response) => {
          if (response.data.DUPLICATE_USER_ID === true) {
            toast.error("User ID " + response.data.USER_ID + " already exist.");
          } else {
            toast.success("User is successfully inserted.");
            props.onReload();
            hideModal();
          }
        })
        .catch((err) => {
          toast.error("Failed to insert user. Please try again.");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading...");
        });
    } catch (err) {
      props.onLoading(false, "Loading...");
      toast.error("Failed to insert user. Please try again.");
      common.c_LogWebError(props.page, functionName, err);
    }
  };

  //#region Fetch Role List
  useEffect(() => {
    let functionName = "";
    try {
      functionName = "Get Role List";
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
  //#endregion

  //#region Fetch Department List
  useEffect(() => {
    let functionName = "";
    try {
      functionName = "Get Department List";
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
        return false;
      } else {
        setInputError((prevState) => ({
          ...prevState,
          [id]: (""),
        }));
        return true;
      }
    } else if (id === "DEFAULT_PASSWORD") {
      var CONFIRM_PASSWORD = addData.CONFIRM_PASSWORD;

      if (CONFIRM_PASSWORD !== val) {
        setInputError((prevState) => ({
          ...prevState,
          CONFIRM_PASSWORD: ("Please confirm the passwords are match."),
        }));
        return false;
      } else {
        setInputError((prevState) => ({
          ...prevState,
          CONFIRM_PASSWORD: (""),
        }));
        return true;
      }
    } else if (id === "CONFIRM_PASSWORD") {
      var DEFAULT_PASSWORD = addData.DEFAULT_PASSWORD;
      if (DEFAULT_PASSWORD !== val) {
        setInputError((prevState) => ({
          ...prevState,
          CONFIRM_PASSWORD: ("Please confirm the passwords are match."),
        }));
        return false;
      } else {
        setInputError((prevState) => ({
          ...prevState,
          CONFIRM_PASSWORD: (""),
        }));
        return true;
      }
    } else {
      return true;
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
      setAddData((prevState) => ({
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
      setAddData((prevState) => ({
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
          USER_ROLE: ("User role could not be empty."),
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
      console.log(val)
      functionName = selectUserDepartmentHandler.name;
      if (val) {
        setInputError((prevState) => ({
          ...prevState,
          USER_DEPARTMENT: (""),
        }));
      } else {
        setInputError((prevState) => ({
          ...prevState,
          USER_DEPARTMENT: ("User department could not be empty."),
        }));
      }
      setDepartmentList([val]);
    } catch (err) {
      common.c_LogWebError(props.page, functionName, err);
    }
  }

  return (
    <>
      <Modal show={props.showHide} onHide={hideModal} centered={true}>
        <Modal.Header closeButton>
          <div style={{ backgroundColor: "#FFDEAD", width: "100%"}}>
            <Modal.Title>User Registration &nbsp; <GiArchiveRegister className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
          </div>
        </Modal.Header>
        <Modal.Body>
          <div>
          <IoIdCard className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="USER ID">User ID :</label>
          <Comp.Input
            ref={userIdInputRef}
            id="USER_ID"
            type="text"
            name="User ID"
            className="form-control"
            errorMessage={inputError.USER_ID}
            value={addData.USER_ID}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <SiNamemc className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="USER NAME">User Name :</label>
          <Comp.Input
            ref={userNameInputRef}
            id="USER_NAME"
            type="text"
            name="User Name"
            className="form-control"
            errorMessage={inputError.USER_NAME}
            value={addData.USER_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <MdEmail className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Email">Email Address :</label>
          <Comp.Input
            ref={userEmailInputRef}
            id="EMAIL"
            type="text"
            name="Email"
            className="form-control"
            errorMessage={inputError.EMAIL}
            value={addData.EMAIL}
            onChange={inputUnrequiredHandler}
          />
          </div>
          <div>
          <FaUserAstronaut className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="User Role">User Roles :</label>
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
          <br></br>
          <div>
          <RiLockPasswordFill className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Password">Default Password :</label>
          <Comp.Input
            ref={userPasswordInputRef}
            id="DEFAULT_PASSWORD"
            type="Password"
            name="Password"
            className="form-control"
            errorMessage={inputError.DEFAULT_PASSWORD}
            value={addData.DEFAULT_PASSWORD}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <GiConfirmed className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Confirm Password">Confirm Password :</label>
          <Comp.Input
            ref={userCPasswordInputRef}
            id="CONFIRM_PASSWORD"
            type="password"
            name="Confirm Password"
            className="form-control"
            errorMessage={inputError.CONFIRM_PASSWORD}
            value={addData.CONFIRM_PASSWORD}
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
              type='general'
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

export default RegisterUser;

