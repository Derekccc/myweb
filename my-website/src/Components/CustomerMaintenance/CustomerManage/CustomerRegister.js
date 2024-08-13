import { useState, useEffect, useRef } from 'react';
import { useCookies } from 'react-cookie';
import { toast } from "react-toastify";
import { Modal } from 'react-bootstrap';
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { GiArchiveRegister } from "react-icons/gi";
import { IoIdCard } from "react-icons/io5";
import { SiNamemc, SiTheboringcompany } from "react-icons/si";
import { MdEmail, MdPhoneIphone, MdAddLocationAlt } from "react-icons/md";
import { FaUserAstronaut } from "react-icons/fa";
import { RiLockPasswordFill } from "react-icons/ri";
import { GiConfirmed } from "react-icons/gi";




const CustomerRegister = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);

  const customerGuidInputRef = useRef();
  const customerNameInputRef = useRef();
  const customerEmailInputRef = useRef();
  const customerPhoneNoInputRef = useRef();
  const customerAddressInputRef = useRef();
  const customerCompanyNameInputRef = useRef();
  // const customerPasswordInputRef = useRef();
  // const customerCPasswordInputRef = useRef();

  // const [roleListToBeSelect, setRoleListToBeSelect] = useState([]);
  // const [roleList, setRoleList] = useState([]);

  const [addData, setAddData] = useState([]);
  const [inputError, setInputError] = useState([]);

  const hideModal = () => {
    // setRoleList([]);
    setAddData([]);
    setInputError([]);
    props.onHide();
  };

  const saveBtnOnClick = () => {
    var flag = true;
    // if (addData.CUSTOMER_ID === undefined) {
    //   flag = false;
    //   setInputError((prevState) => ({
    //     ...prevState,
    //     CUSTOMER_ID: (" CUSTOMER ID could not be empty."),
    //   }));
    // }
    if (addData.CUSTOMER_NAME === undefined) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        CUSTOMER_NAME: (" Customer Name could not be empty."),
      }));
    }

    if (addData.EMAIL !== undefined) {
      flag = furtherValidateInput("EMAIL", addData.EMAIL) && flag;
    }
    if (addData.PHONE_NO !== undefined) {
      flag = furtherValidateInput("PHONE_NO", addData.PHONE_NO) && flag;
    }
    if (addData.ADDRESS === undefined) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        ADDRESS: (" Address could not be empty."),
      }));
    }
    if (addData.COMPANY_NAME === undefined) {
      flag = false;
      setInputError((prevState) => ({
        ...prevState,
        COMPANY_NAME: (" Company Name could not be empty."),
      }));
    }
    // if (Object.keys(roleList).length === 0) {
    //   flag = false;
    //   setInputError((prevState) => ({
    //     ...prevState,
    //     USER_ROLE: ("User role could not be empty."),
    //   }));
    // } else {
    //   setInputError((prevState) => ({
    //     ...prevState,
    //     USER_ROLE: (""),
    //   }));
    // }
    // if (addData.DEFAULT_PASSWORD === undefined) {
    //   flag = false;
    //   setInputError((prevState) => ({
    //     ...prevState,
    //     DEFAULT_PASSWORD: "Password could not be empty.",
    //   }));
    // } else {
    //   flag = furtherValidateInput("DEFAULT_PASSWORD", addData.DEFAULT_PASSWORD)
    // }
    if (Object.values(inputError).filter(v => v !== "").length === 0 && flag) {
      RegisterNewUser();
    }
  }

  function RegisterNewUser() {
    let functionName = "";
    try {
      functionName = props.page + RegisterNewUser.name;
      props.onLoading(true, "Registering user, please wait...");
      // var encrypted = common.c_EncryptData(addData.DEFAULT_PASSWORD.trim());
      // const selectedRoleList = roleList.map(r => r.key);
      const data = {
        CUSTOMER_ID: addData.CUSTOMER_ID,
        CUSTOMER_NAME: addData.CUSTOMER_NAME.trim(),
        EMAIL: addData.EMAIL !== undefined ? addData.EMAIL.trim() : "",
        PHONE_NO: addData.PHONE_NO !== undefined ? addData.PHONE_NO.trim() : "",
        ADDRESS: addData.ADDRESS.trim(),
        COMPANY_NAME: addData.COMPANY_NAME.trim(),
        // USERROLE_ID: addData.USER_CATEGORY.key,
        // DEFAULT_PASSWORD: encrypted.toString(),
        UPDATE_ID: userId,
        FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
        // USER_ROLE_LIST: selectedRoleList,
      };
      http
        .post("api/customer/RegisterCustomer", data, { timeout: 10000 })
        .then((response) => {
          if (response.data.DUPLICATE_CUSTOMER_ID === true) {
            toast.error("Customer ID " + response.data.CUSTOMER_ID + " already exist.");
          } 
          else if (response.data.DUPLICATE_CUSTOMER_NAME === true) {
            toast.error("Customer Name " + response.data.CUSTOMER_NAME + " already exist.");
          }
          // else if (response.data.DUPLICATE_CUSTOMER_EMAIL === true) {
          //   toast.error("Customer Email " + response.data.EMAIL + " already exist.");
          // }
          // else if (response.data.DUPLICATE_CUSTOMER_PHONE_NO === true) {
          //   toast.error("Customer Phone No " + response.data.PHONE_NO + " already exist.");
          // }
          else {
            toast.success("Customer is successfully inserted.");
            props.onReload();
            hideModal();
          }
        })
        .catch((err) => {
          toast.error("Failed to insert customer. Please try again.");
          common.c_LogWebError(props.page, functionName, err);
        })
        .finally(() => {
          props.onLoading(false, "Loading...");
        });
    } catch (err) {
      props.onLoading(false, "Loading...");
      toast.error("Failed to insert customer. Please try again.");
      common.c_LogWebError(props.page, functionName, err);
    }
  };

  // useEffect(() => {
  //   let functionName = "";
  //   try {
  //     functionName = "Get Role List";
  //     http
  //       .get(
  //         "api/user/GetRoleList?_sysUser=" + userId
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
    } else if (id === "PHONE_NO") {
      if (!common.PHONE_NUMBER_REGEX.test(val) && val !== "") {
        setInputError((prevState) => ({
          ...prevState,
          [id]: ("Please enter a proper phone number."),
        }));
        return false;
      } else {
        setInputError((prevState) => ({
          ...prevState,
          [id]: (""),
        }));
        return true;
      }
    // } else if (id === "DEFAULT_PASSWORD") {
    //   var CONFIRM_PASSWORD = addData.CONFIRM_PASSWORD;

    //   if (CONFIRM_PASSWORD !== val) {
    //     setInputError((prevState) => ({
    //       ...prevState,
    //       CONFIRM_PASSWORD: ("Please confirm the passwords are match."),
    //     }));
    //     return false;
    //   } else {
    //     setInputError((prevState) => ({
    //       ...prevState,
    //       CONFIRM_PASSWORD: (""),
    //     }));
    //     return true;
    //   }
    // } else if (id === "CONFIRM_PASSWORD") {
    //   var DEFAULT_PASSWORD = addData.DEFAULT_PASSWORD;
    //   if (DEFAULT_PASSWORD !== val) {
    //     setInputError((prevState) => ({
    //       ...prevState,
    //       CONFIRM_PASSWORD: ("Please confirm the passwords are match."),
    //     }));
    //     return false;
    //   } else {
    //     setInputError((prevState) => ({
    //       ...prevState,
    //       CONFIRM_PASSWORD: (""),
    //     }));
    //     return true;
    //   }
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
  //         USER_ROLE: ("User role could not be empty."),
  //       }));
  //     }
  //     setRoleList([...val]);
  //   } catch (err) {
  //     common.c_LogWebError(props.page, functionName, err);
  //   }
  // }


  return (
    <>
      <Modal show={props.showHide} onHide={hideModal} centered={true}>
        <Modal.Header closeButton>
          <div style={{ backgroundColor: "#FFDEAD", width: "100%"}}>
            <Modal.Title>Customer Registration &nbsp; <GiArchiveRegister className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
          </div>
        </Modal.Header>
        <Modal.Body>
          {/* <div>
          <IoIdCard className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Customer GUID">Customer ID :</label>
          <Comp.Input
            ref={customerIdInputRef}
            id="CUSTOMER_GUID"
            type="text"
            name="Customer GUID"
            className="form-control"
            errorMessage={inputError.CUSTOMER_ID}
            value={addData.CUSTOMER_ID}
            onChange={inputRequiredHandler}
          />
          </div> */}
          <div>
          <SiNamemc className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Customer Name">Customer Name *:</label>
          <Comp.Input
            ref={customerNameInputRef}
            id="CUSTOMER_NAME"
            type="text"
            name="Customer Name"
            className="form-control"
            errorMessage={inputError.CUSTOMER_NAME}
            value={addData.CUSTOMER_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <SiTheboringcompany className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Company Name">Company Name *:</label>
          <Comp.Input
            ref={customerCompanyNameInputRef}
            id="COMPANY_NAME"
            type="text"
            name="Company"
            className="form-control"
            errorMessage={inputError.COMPANY_NAME}
            value={addData.COMPANY_NAME}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <MdEmail className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Email">Email Address *:</label>
          <Comp.Input
            ref={customerEmailInputRef}
            id="EMAIL"
            type="text"
            name="Email"
            className="form-control"
            errorMessage={inputError.EMAIL}
            value={addData.EMAIL}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <MdPhoneIphone className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Phone No">Phone Number *:</label>
          <Comp.Input
            ref={customerPhoneNoInputRef}
            id="PHONE_NO"
            type="text"
            name="Phone No"
            className="form-control"
            errorMessage={inputError.PHONE_NO}
            value={addData.PHONE_NO}
            onChange={inputRequiredHandler}
          />
          </div>
          <div>
          <MdAddLocationAlt className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Address">Address *:</label>
          <Comp.Input
            ref={customerAddressInputRef}
            id="ADDRESS"
            type="text"
            name="Address"
            className="form-control"
            errorMessage={inputError.ADDRESS}
            value={addData.ADDRESS}
            onChange={inputRequiredHandler}
          />
          </div>
          
          {/* <div>
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
          </div> */}
          {/* <br/>
          <div>
          <RiLockPasswordFill className='icon' style={{marginBottom: '5px', fontSize: '20px', marginRight: '10px'}}/>
          <label label="Password">Default Password :</label>
          <Comp.Input
            ref={customerPasswordInputRef}
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
            ref={customerCPasswordInputRef}
            id="CONFIRM_PASSWORD"
            type="password"
            name="Confirm Password"
            className="form-control"
            errorMessage={inputError.CONFIRM_PASSWORD}
            value={addData.CONFIRM_PASSWORD}
            onChange={inputRequiredHandler}
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
              type='general'
              onClick={saveBtnOnClick}
            >
              Add
            </Comp.Button>
          </div>
        </Modal.Footer>
      </Modal>
    </>
  );
};

export default CustomerRegister;

