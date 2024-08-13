import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCookies } from 'react-cookie';
// import * as common from "../Common/common";
import "../Styles/common.css";
import * as Comp from "../Common/CommonComponents";
import LoginResetPassword from './LoginModels/LoginResetPassword';
import Login from './LoginModels/Login';


const PAGE_NAME = "LoginMain.js_";
const MODULE_ID = "LoginMain";


const LoginMain = () => {
  const navigate = useNavigate();
  const [cookies, setCookies, removeCookies] = useCookies([]); 
  
  const [showLogin, setShowLogin] = useState(true);
  const [showReset, setShowReset] = useState(false);
  const [resetData, setResetData] = useState([]);

  const [loading, setLoading] = useState(false);
  const [loadingText, setLoadingText] = useState("Loading...");


  const showHideLoginModal = () => {
    setShowLogin(!showLogin);
  };

  const showHideResetModal = () => {
    setShowReset(!showReset);
    setShowLogin(!showLogin);
  };

  const onLoadingHandler = (load, text) => {
    setLoading(load);
    setLoadingText(text);
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


  return (
    <div style={backgroundStyle}>
      
        <Login
          page={PAGE_NAME}
          module={MODULE_ID}
          showHide={showLogin}
          onHide={showHideLoginModal}
          onLoading={onLoadingHandler}
          setCookies={setCookies}
          removeCookies={removeCookies}
          setResetData={setResetData}
          showHideResetModal={showHideResetModal}
        />
      
        <LoginResetPassword
          page={PAGE_NAME}
          module={MODULE_ID}
          showHide={showReset}
          editData={resetData}
          onHide={showHideResetModal}
          onLoading={onLoadingHandler}
          setCookies={setCookies}
        />
      

      {loading && <Comp.Loading>{loadingText}</Comp.Loading>}
    </div>
  );
};

export default LoginMain;
