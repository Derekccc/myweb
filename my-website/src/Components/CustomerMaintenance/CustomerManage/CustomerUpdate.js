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
// import { FaUserAstronaut } from "react-icons/fa";
import { MdPhoneIphone } from "react-icons/md";
import { FaLocationDot } from "react-icons/fa6";
import { SiTheboringcompany } from "react-icons/si";


const CustomerUpdate = (props) => {
  //#region React Hooks
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const customerIdInputRef = useRef();
  const customerNameInputRef = useRef();
  const customerEmailInputRef = useRef();
  const customerPhoneNoInputRef = useRef();
  const customerAddressInputRef = useRef();
  const customerCompanyNameInputRef = useRef();

  // const [roleListToBeSelect, setRoleListToBeSelect] = useState([]);
  // const [roleList, setRoleList] = useState([]);

  const [editData, setEditData] = useState([]);
  const [inputError, setInputError] = useState([]);

  useEffect(() => {
    if (props.editData !== null) {
      setEditData({
        CUSTOMER_ID: props.editData.CUSTOMER_ID === null ? "" : props.editData.CUSTOMER_ID,
        CUSTOMER_NAME: props.editData.CUSTOMER_NAME === null ? "" : props.editData.CUSTOMER_NAME,
        EMAIL: props.editData.EMAIL === null ? "" : props.editData.EMAIL,
        PHONE_NO: props.editData.PHONE_NO === null ? "" : props.editData.PHONE_NO,
        ADDRESS: props.editData.ADDRESS === null ? "" : props.editData.ADDRESS,
        COMPANY_NAME: props.editData.COMPANY_NAME === null ? "" : props.editData.COMPANY_NAME,

        // ROLE_ID: props.editData.ROLE_ID === null ? "" : props.editData.ROLE_ID,
        // ROLE_NAME: props.editData.ROLE_NAME === null ? "" : props.editData.ROLE_NAME,
      })
      // for select role dropdown
      // const roleId = props.editData.ROLE_ID === null || props.editData.ROLE_ID === undefined ? "" : props.editData.ROLE_ID.split(",");
      // const roleName = props.editData.ROLE_NAME === null || props.editData.ROLE_NAME === undefined ? "" : props.editData.ROLE_NAME.split(",");
      // const role = [];
      // for (var i = 0; i < roleId.length; i++) {
      //   role.push({
      //     key: roleId[i],
      //     value: roleId[i],
      //     label: roleName[i],
      //   });
      // }
      // setRoleList(role);
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
      saveEditCustomer();
    }
  }

  //#region Save Edit
  function saveEditCustomer() {
    let functionName = "";
    try {
      functionName = saveEditCustomer.name;
      props.onLoading(true, "Updating customer, Please wait....");
      // const selectedRoleList = roleList.map(r => r.key);
      const data = {
        CUSTOMER_ID: editData.CUSTOMER_ID.trim(),
        CUSTOMER_NAME: editData.CUSTOMER_NAME.trim(),
        EMAIL: editData.EMAIL !== undefined ? editData.EMAIL.trim() : "",
        PHONE_NO: editData.PHONE_NO !== undefined ? editData.PHONE_NO : "",
        ADDRESS: editData.ADDRESS.trim(),
        COMPANY_NAME: editData.COMPANY_NAME.trim(),
        FROM_SOURCE: {SOURCE: "WEB", MODULE_ID: props.module },
        UPDATE_ID: userId,
        // USER_ROLE_LIST: selectedRoleList,
      };
      http
        .put("api/customer/UpdateCustomer", data, {timeout: 10000})
        .then((response) => {
          toast.success("Customer is successfully Updated.");
          props.onReload();
          hideModal();
        })
        .catch((err) => {
          toast.error("Failed to update customer. Please try again.....(send)");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading....");
        });
    } catch (err) {
      props.onLoading(false, "Loading...");
      toast.error("Failed to update customer. Please try again.....(receive)");
      common.c_LogWebError(props.page, functionName, err);
    }
  };
  //#endregion

  //#region useEffect to get role list
  // useEffect(() => {
  //   let functionName = "";

  //   try {
  //     functionName = "Get Role Name";
  //     http
  //       .get(
  //         "api/customer/GetRoleList?_sysUser=" + userId
  //       )
  //       .then((response) => {
  //         const rolelist = [];
  //         for (var i = 0; i < response.data.length; i++) {
  //           rolelist.push({
  //             key: response.data[i].ROLE_ID,
  //             value: response.data[i].ROLE_ID,
  //             label: response.data[i].ROLE_NAME,
  //           });
  //         }
  //         setRoleListToBeSelect(rolelist);
  //       })
  //       .catch((err) => {
  //         common.c_LogWebError(props.page, functionName, err);
  //       });
  //   } catch (err) {
  //     common.c_LogWebError(props.page, functionName, err);
  //   }
  // }, []);

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
    } else if (id === "PHONE_NO") {
      if (!common.PHONE_NUMBER_REGEX.test(val) && val !== "") {
        setInputError((prevState) => ({
          ...prevState,
          [id]: "Please enter a valid phone number. Example: 011-2345673",
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

  // const selectUserRoleHandler = (val) => {
  //   let functionName = "";

  //   try {
  //     functionName = selectUserRoleHandler.name;
  //     if (Object.keys(val).length > 0) {
  //       setInputError((prevState) => ({
  //         ...prevState,
  //         USER_ROLE: (""),
  //       }));
  //     } else {
  //       setInputError((prevState) => ({
  //         ...prevState,
  //         USER_ROLE: ("User role could not be empty or blank. Thank you."),
  //       }));
  //     }
  //     setRoleList([...val]);
  //   } catch (err) {
  //     common.c_LogWebError(props.page, functionName, err);
  //   }
  // }


  return (
    <>
    <Modal show={props.showHide} onHide={hideModal} centered>
        <Modal.Header closeButton>
          <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>
            <Modal.Title>UPDATE &nbsp; <GrUpdate className='icon' style={{ fontSize: '20px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
          </div>
        </Modal.Header>
        <Modal.Body>
          {/* <div>
          <BiSolidUserPin className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="User ID">Customer ID :</label>
          <Comp.Input
            ref={customerIdInputRef}
            id="CUSTOMER_ID"
            type="text"
            className="form-control"
            value={editData.CUSTOMER_ID}
            readOnly={true}
          />
          </div> */}
          <div>
          <BiSolidRename className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="User Name">Customer Name :</label>
          <Comp.Input
            ref={customerNameInputRef}
            id="CUSTOMER_NAME"
            name="Customer Name"
            type="text"
            className="form-control"
            errorMessage={inputError.CUSTOMER_NAME}
            value={editData.CUSTOMER_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <SiTheboringcompany className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Company Name">Company Name :</label>
          <Comp.Input
            ref={customerCompanyNameInputRef}
            id="COMPANY_NAME"
            nam="Company Name"
            type="text"
            className="form-control"
            errorMessage={inputError.COMPANY_NAME}
            value={editData.COMPANY_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <AiTwotoneMail className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Email">Email :</label>
          <Comp.Input
            ref={customerEmailInputRef}
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
          <MdPhoneIphone className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Phone No">PHONE NUMBER :</label>
          <Comp.Input
            ref={customerPhoneNoInputRef}
            id="PHONE_NO"
            name="Phone No"
            type="text"
            className="form-control"
            errorMessage={inputError.PHONE_NO}
            value={editData.PHONE_NO}
            onChange={inputUnrequiredHandler}
          />
          </div>
          <div>
          <FaLocationDot className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Address">Address :</label>
          <Comp.Input
            ref={customerAddressInputRef}
            id="ADDRESS"
            name="Address"
            type="text"
            className="form-control"
            errorMessage={inputError.ADDRESS}
            value={editData.ADDRESS}
            onChange={inputRequiredHandler}
          />
          </div>
          
          {/* <div>
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
          </div> */}

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

export default CustomerUpdate;
