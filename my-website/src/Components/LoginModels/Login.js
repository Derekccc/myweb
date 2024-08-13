import { useState, useEffect, useRef } from 'react';
import { FaUserTie } from "react-icons/fa";
import { RiLockPasswordFill } from "react-icons/ri";
import { FaRegEye, FaEyeSlash } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import http from "../../Common/http-common";
import * as common from "../../Common/common";
import * as Comp from "../../Common/CommonComponents";
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import ReactDOM from 'react-dom';

const PAGE_NAME = "Login.js_";

const LoadingModal = ({ children }) => {
  return ReactDOM.createPortal(
    <div className="loading-modal">
      {children}
    </div>,
    document.getElementById('modal-root')
  );
};

const Login = (props) => {
  const navigate = useNavigate();
  const userIdInputRef = useRef();
  const userPasswordInputRef = useRef();

  const [projectName, setProjectName] = useState("");
  const [loading, setLoading] = useState(false);
  const [loadingText, setLoadingText] = useState("Loading...");

  const [loginData, setLoginData] = useState({});
  const [inputError, setInputError] = useState([]);
  const [errorMsg, setErrorMsg] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  // keyboard enter
  useEffect(() => {
    var inputText = document.getElementById("PASSWORD");
    var loginBtn = document.getElementById("btnLogin");
    if (inputText && loginBtn) {
      inputText.onkeydown = (e) => { if (e.key === "Enter") loginBtn.click(); }
    }
  }, []);

  useEffect(() => {
    let functionName = "useEffect Get Project Name";
    try {
      setLoading(true);
      setLoadingText("Getting Project Name, please wait...");
      http
        .get("api/WebCommon/GetProjectName?_userId=Derek", { timeout: 10000 })
        .then((response) => {
          setProjectName(response.data[0].POLICY_VALUE);
        })
        .catch((err) => {
          toast.error("Error on getting prod name. Please try again.");
          common.c_LogWebError(PAGE_NAME, functionName, err);
        })
        .finally(() => {
          setLoading(false);
          setLoadingText("Loading...");
        });
    } catch (err) {
      toast.error("Error on getting prod name. Please try again.");
      common.c_LogWebError(PAGE_NAME, functionName, err);
    }

    // Check for stored credentials
    const storedUserId = localStorage.getItem("rememberMeUserId");
    const storedPassword = localStorage.getItem("rememberMePassword");
    if (storedUserId && storedPassword) {
      setLoginData({ USER_ID: storedUserId, PASSWORD: storedPassword });
      setRememberMe(true);
    }
  }, []);

  const hideModal = () => {
    setLoginData({});
    setInputError([]);
    setErrorMsg("");
    props.onHide();
  };

  const loginBtnOnClick = () => {
    console.log("Button is Click");
    if (loginData.USER_ID === undefined) {
      setInputError((prevState) => ({
        ...prevState,
        USER_ID: "User ID could not be empty.",
      }));
    }
    if (loginData.PASSWORD === undefined) {
      setInputError((prevState) => ({
        ...prevState,
        PASSWORD: "Password could not be empty.",
      }));
    }
    if (Object.values(inputError).filter(v => v !== "").length === 0) {
      console.log("go to function");
      UserLogin();
    }
  };

  function UserLogin() {
    console.log("start running function");
    props.removeCookies();
    if (loginData.USER_ID !== undefined && loginData.PASSWORD !== undefined) {
      let functionName = "User Login";
      try {
        props.onLoading(true, "Logging in, please wait...");
        var encrypted = common.c_EncryptData(loginData.PASSWORD.trim());
        const data = {
          USER_ID: loginData.USER_ID.trim(),
          PASSWORD: encrypted.toString(),
          FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
        };
        http
          .post("api/login/UserLogin", data, { timeout: 100000 })
          .then((response) => {
            if (response.data.VALID === "LOGIN_SUCCESS") {
              props.setCookies('USER_NAME', response.data.USER_NAME, { path: '/' });
              props.setCookies('USER_ID', response.data.USER_ID, { path: '/' });
              props.setCookies('AUTO_LOGOUT_DURATION', response.data.AUTO_LOGOUT_DURATION, { path: '/' });

              if (rememberMe) {
                localStorage.setItem("rememberMeUserId", loginData.USER_ID);
                localStorage.setItem("rememberMePassword", loginData.PASSWORD);
              } else {
                localStorage.removeItem("rememberMeUserId");
                localStorage.removeItem("rememberMePassword");
              }

              window.location = common.c_getWebUrl() + "Home";
              hideModal();
            } else if (response.data.VALID === "RESET") {
              props.setResetData({
                USER_ID: response.data.USER_ID,
                USER_NAME: response.data.USER_NAME,
                ERROR_MSG: response.data.ERROR_MSG,
              });
              hideModal();
              props.showHideResetModal();
            } else {
              setErrorMsg(response.data.ERROR_MSG);
            }
          })
          .catch((err) => {
            console.log("Login failed:", err);
            setErrorMsg("Failed to log in. Please try again.");
            common.c_LogWebError(props.page, functionName, err);
          })
          .finally(() => {
            console.log('Here is finally, Loading...');
            props.onLoading(false, "Loading...");
          });
      } catch (err) {
        console.error("Error during login:", err);
        props.onLoading(false, "Loading...");
        setErrorMsg("Failed to log in. Please try again.");
        common.c_LogWebError(props.page, functionName, err);
      }
    }
  }

  const inputHandleChange = (e) => {
    let functionName = "inputHandleChange";
    try {
      const name = e.target.name;
      const id = e.target.id;
      const val = e.target.value;
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
      setLoginData((prevState) => ({
        ...prevState,
        [id]: val,
      }));
    } catch (err) {
      common.c_LogWebError(props.page, functionName, err);
    }
  };

  const handleRememberMeChange = () => {
    setRememberMe(!rememberMe);
  };

  // CSS styles
  const backgroundStyle = {
    position: 'absolute',
    top: 0,
    left: 0,
    width: '100%',
    height: '100%',
    background: `url('/images/image-background.jpg')`,
    backgroundSize: 'cover',
    backgroundRepeat: 'no-repeat',
    backgroundPosition: 'center',
    zIndex: -1,
  };

  const formContainer = {
    background: 'transparent',
    backdropFilter: 'blur(20px)',
    padding: '20px',
    boxShadow: '0px 0px 10px rgba(0, 0, 0, 0.1)', 
    maxWidth: '400px', 
    margin: '0 auto', 
    position: 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
  };

  const headingStyle = {
    color: '#DEB887',
    paddingTop: '20px',
  };

  const imgStyle = {
    maxWidth: '15%',
    height: 'auto',
  };

  const inputTextStyle = {
    paddingRight: '225px',
    color: '#D2B48C',
  };

  const inputTextStyle1 = {
    paddingRight: '210px',
    color: '#D2B48C',
  };

  const inputStyle = {
    width: '100%',
    padding: '12px',
    margin: '6px 0',
    display: 'inline-block',
    border: '1px solid #ccc',
    boxSizing: 'border-box',
    paddingRight: '35px',
  };

  const loginBtn = {
    backgroundColor: '#04AA6D',
    color: 'white',
    fontSize: '15px',
    paddingTop: '15px',
    paddingRight: '120px',
    paddingBottom: '15px',
    paddingLeft: '100px',
    border: 'none',
    cursor: 'pointer',
  };

  const rememberMeStyle = {
    color: '#BDB76B',
  };

  const ColoredLine = ({ color }) => (
    <hr
      style={{
        color: color,
        backgroundColor: color,
        height: 0.5
      }}
    />
  );

  return (
    <div style={backgroundStyle}>
      <div className='wrapper'>  
        <form style={formContainer}>
          <div className="imgcontainer">
            <img src="/images/img-avatar.png" alt="Avatar" style={imgStyle} className="avatar"></img>
          </div>
          <br/>
          <h4 style={{color: '#E0FFFF'}}>{projectName}</h4>
          <ColoredLine color="white" />
          <div id="root"></div>
          <div id='modal-root'></div>
          <div className='input-box' style={inputTextStyle}>
            <FaUserTie className='icon' style={{marginBottom: '10px', marginRight: '10px', fontSize: '20px'}}/>
            <label><b>User ID:</b></label>
          </div> 
          <div>
            <input 
              ref={userIdInputRef} 
              id='USER_ID' 
              type="text" 
              name="USER ID"
              className='form-control' 
              placeholder='Enter User ID' 
              style={inputStyle}
              errorMessage={inputError.PASSWORD}
              value={loginData.USER_ID} 
              onChange={inputHandleChange} 
            />
          </div>

          <div className='input-box' style={inputTextStyle1}>
            <RiLockPasswordFill className='icon' style={{marginBottom: '10px', marginRight: '10px', fontSize: '20px'}}/>
            <label><b>Password:</b></label>
          </div>
          <div style={{position: 'relative'}}>
            <input 
              ref={userPasswordInputRef} 
              id='PASSWORD' 
              name="Password" 
              type={showPassword ? 'text' : 'password'}
              className='form-control'
              placeholder='Enter Password'
              style={inputStyle}
              errorMessage={inputError.PASSWORD}
              value={loginData.PASSWORD} 
              onChange={inputHandleChange} 
            />
              {showPassword ? (
                <FaRegEye 
                  className='icon' 
                  style={{ position: 'absolute', right: '10px', top: '50%', transform: 'translateY(-50%)', cursor: 'pointer', paddingRight: '5px', fontSize: '25px' }}
                  onClick={togglePasswordVisibility}
                />
              ) : (
                <FaEyeSlash
                  className='icon' 
                  style={{ position: 'absolute', right: '10px', top: '50%', transform: 'translateY(-50%)', cursor: 'pointer', paddingRight: '5px', fontSize: '25px' }}
                  onClick={togglePasswordVisibility}
                />
              )}
          </div>
          &nbsp;
          <div className='remember-me'>
            <label style={rememberMeStyle}>
              <input type='checkbox' name='rememberMe' checked={rememberMe} onChange={handleRememberMeChange} /> Remember Me
            </label>
          </div>
          <h6 style={{color: 'red'}}>{errorMsg}</h6>
          <ColoredLine color="white" />
          <div style={{position: 'relative'}}>     
            <button 
              id='btnLogin'
              type="button" 
              onClick={loginBtnOnClick} 
              style={loginBtn} 
              onMouseOver={(e) => {e.target.style.backgroundColor = '#04AA6D' ; e.target.style.color = 'white'}} 
              onMouseOut={(e) => {e.target.style.backgroundColor = '#A9A9A9' ; e.target.style.color = 'black' }}
            >
              Login
            </button>
          </div>
        </form>
        {loading && <LoadingModal>{loadingText}</LoadingModal>}
        <Comp.AlertPopup />
      </div>
    </div>
  );
};

export default Login;
