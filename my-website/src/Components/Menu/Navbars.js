import { useEffect, useState } from "react";
import { NavLink, Link } from "react-router-dom";
import { useCookies } from "react-cookie";
import { Nav, Navbar, NavDropdown } from "react-bootstrap";
import classes from "./Navbar.css";
import { TbLogout2 } from "react-icons/tb";
import * as common from "../../Common/common";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faHouse,
  faUserGear,

} from "@fortawesome/free-solid-svg-icons";
import {
  moduleBinary,
  GetUserAuthorityFn,
  GetModuleList,
} from "../../Common/authority";
import "./Navbar.css";

const PAGE_NAME = "Navbars.js";

const Navbars = ({ isLoggedIn, handleLogout }) => {
  const [cookies] = useCookies([]);
  const [moduleCatBinary, setModuleCatBinary] = useState({
    SystemSetup: 0b0000000000000000,
    UserTabletModule: 0b0000000000000000,
    ConsumerTabletModule: 0b0000000000000000,
  });

  const [collapseNavLinks, setCollapseNavLinks] = useState(false);

  const CatUserTabletSetup = [
    {
      key: "/userhomepage",
      title: "User Management",
    },
    {
      key: "/rolehomepage",
      title: "Role Management",
    },
    {
      key: "/departmenthomepage",
      title: "Department Management",
    },
    {
      key: "/authorityrolessetting",
      title: "Authority Management",
    },
  
  ];

  const CatConsumerSetup = [
    {
      key: "/customerhomepage",
      title: "Customer Management",
    },
    {
      key: "/producthomepage",
      title: "Product Management",
    },
    {
      key: "/salesorderhomepage",
      title: "Sales Order Management",
    },
    {
      key: "/salescharthomepage",
      title: "Sales Chart Management",
    },
    // Add more consumer setup modules as needed
  ];

  useEffect(() => {
    console.log("Cookies:", cookies); 
    console.log("USER_ID:", cookies.USER_ID);
    if (
      Object.keys(cookies).length !== 0 &&
      cookies.USER_ID.trim() !== ""
    ) {
      let promise = GetUserAuthorityFn(
        cookies.USER_ID,
        "CAT_USER_TABLET_SETUP",
        GetModuleList("CAT_USER_TABLET_SETUP")
      );

      promise.then(
        
        (result) => {
          console.log("promise:", promise);
          console.log("User authority result:", result);
          setModuleCatBinary((preState) => ({
            ...preState,
            UserTabletModule: result,
          }));
        },
        (error) => {
          console.error("Error fetching user authority:", error);
          setModuleCatBinary((prevState) => ({
            ...prevState,
            UserTabletModule: 0,
          }));
          common.c_LogWebError(PAGE_NAME, "GetUserAuthorityFn", error);
        }
      );
    }
  }, [cookies]);

  useEffect(() => {
    if (
      Object.keys(cookies).length !== 0 &&
      cookies.USER_ID.trim() !== ""
    ) {
      let promise = GetUserAuthorityFn(
        cookies.USER_ID,
        "CAT_CONSUMER_TABLET_SETUP",
        GetModuleList("CAT_CONSUMER_TABLET_SETUP")
      );

      promise.then(
        
        (result) => {
          console.log("promise:", promise);
          console.log("User authority result:", result);
          setModuleCatBinary((preState) => ({
            ...preState,
            ConsumerTabletModule: result,
          }));
        },
        (error) => {
          console.error("Error fetching user authority:", error);
          setModuleCatBinary((prevState) => ({
            ...prevState,
            ConsumerTabletModule: 0,
          }));
          common.c_LogWebError(PAGE_NAME, "GetUserAuthorityFn", error);
        }
      );
    }
  }, [cookies]);

  const handleNavToggle = () => {
    setCollapseNavLinks(!collapseNavLinks);
  };

  const ColoredLine = ({ color }) => (
    <hr
      style={{
        color: color,
        backgroundColor: color,
        margin: "1px",
      }}
    />
  );

  return (
    <>
      <Navbar className="top-navbar">
        <div className="navbar-logo">
          <Link to="/" className="navbar-logo-link">
            <img src="/images/Superman.jpeg" alt="Logo" className="logo-img" />
          </Link>
        </div>
      </Navbar>
      <ColoredLine color="black" />


      <Navbar expand="lg" className="bg-body-tertiary">
        {/* <Container> */}
          {/* burger menu */}
        <div className="navbar-left" >
          <div className="navbar-toggle" onClick={handleNavToggle}>
            <div className={`line ${collapseNavLinks ? "open" : ""}`}></div>
            <div className={`line ${collapseNavLinks ? "open" : ""}`}></div>
            <div className={`line ${collapseNavLinks ? "open" : ""}`}></div>
          </div>
        </div>
      {moduleCatBinary.UserTabletModule !== 0 && (
          <>
          {collapseNavLinks && (
            <NavDropdown title="USER SETUP" id="basic-nav-dropdown">
              <NavDropdown.Item eventKey="0">
                <NavDropdown.Item className="dropdown-body">
                {CatUserTabletSetup.map((module, index) => {
                          return (moduleCatBinary.UserTabletModule &
                            moduleBinary[index]) ===
                            moduleBinary[index] ? (
                            <NavLink
                              to={`${module.key}`}
                              className={({ isActive }) =>
                               "nav-link" + (isActive ? " activated" : "") + " nav-link-with-border"
                              }
                              key={`${module.key}`}
                            >
                              {module.title}
                            </NavLink>
                          ) : (
                            <></>
                          );
                        })}
                </NavDropdown.Item>
              </NavDropdown.Item>
          </NavDropdown>

          )}
        </>
      )} 
      {moduleCatBinary.ConsumerTabletModule !== 0 && (
          <>
            {collapseNavLinks && (
              <NavDropdown
                title="CONSUMER SETUP"
                id="basic-nav-dropdown"
              >
                <NavDropdown.Item eventKey="0">
                  <NavDropdown.Item className="dropdown-body">
                    {CatConsumerSetup.map((module, index) =>
                      (moduleCatBinary.ConsumerTabletModule & moduleBinary[index]) ===
                      moduleBinary[index] ? (
                        <NavLink
                          to={`${module.key}`}
                          className={({ isActive }) =>
                            "nav-link" +
                            (isActive ? " activated" : "") +
                            " nav-link-with-border"
                          }
                          key={`${module.key}`}
                        >
                          {module.title}
                        </NavLink>
                      ) : (
                        <></>
                      )
                    )}
                  </NavDropdown.Item>
                </NavDropdown.Item>
              </NavDropdown>
            )}
          </>
        )}
        &nbsp;&nbsp;
          <Link 
            to="/" 
            className="navbar-brand"
            >
              <FontAwesomeIcon icon={faHouse} /> &nbsp;
              <b>My-Website</b>
          </Link>

            <div className="ms-auto">
              <Nav >
                {isLoggedIn && (
                  <>
                  <span className="navbar-text">
                    Welcome, {cookies.USER_ID} - {cookies.USER_NAME}
                  </span>
                  <Nav.Link
                    to="/logout"
                    className="navbar-brand"
                    // style={{float:'right'}}
                    onClick={handleLogout}
                  >
                    <TbLogout2 className="icon" />
                    <b>Logout</b>
                  </Nav.Link>
                  </>
                )}
              
              </Nav>
            </div>
        {/* </Container> */}
      </Navbar>


    </>
  );
};

export default Navbars;
