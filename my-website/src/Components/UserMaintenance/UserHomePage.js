import { useState, useEffect} from "react";
import { useCookies } from "react-cookie";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { GetPageAuthorityFn } from "../../Common/authority";
import http from "../../Common/http-common";
import * as common from "../../Common/common";
import * as Comp from "../../Common/CommonComponents";
import CssClass from "../../Styles/common.module.css";
import UserList from "./UserManage/UserList";
import RegisterUser from "./UserManage/RegisterUser";
import { IoIosPersonAdd } from "react-icons/io";
import { TbTableExport } from "react-icons/tb";

const PAGE_NAME = "UserHomePage.js_";
const MODULE_ID = "WEB_UT_001";


const UserHomePage = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const navigate = useNavigate();

  const [allData, setAllData] = useState([]);
  const [loading, setLoading] = useState([]);
  const [loadingText, setLoadingText] = useState("Loading....");
  const [reload, setReload] = useState(false);

  const [showAdd, setShowAdd] = useState(false);

  const [roleType, setRoleType] = useState([]);
  const [roleTypeData, setRoleTypeData]= useState([]);
  const [searchData, setSearchData] = useState([]);

  const [departmentType, setDepartmentType] = useState([]);

  const [actionAuthority, setActionAuthority] = useState({
    READ: "N",
    WRITE: "N",
  });

  useEffect(() => {
    let promise = GetPageAuthorityFn(userId, MODULE_ID);
    promise.then(
      (result) => {
        setActionAuthority(result);
        if (result.READ === "N") {
          navigate("/");
        }
      },
      (error) => {
        setActionAuthority({
          READ: "N",
          WRITE: "N",
        });
        common.c_LogWebError(PAGE_NAME, "GetpageAuthorityFn", error);
        toast.error(error);
        navigate("/");
      }
    );
  }, [userId, navigate]);

  useEffect(() => {
    if(actionAuthority.READ === "Y"){
      let functionName = "";

      try {
        functionName = "useEffect Get All Users";

        setLoading(true);
        setLoadingText("Getting Users, please wait...");

        const listItems = [];
        const roletypeSelected = 
          roleType !== undefined && Object.keys(roleType).length !== 0
            ?roleType.map((r) => r.key)
            : "";
         // for department
        const departmenttypeSelected = 
          departmentType !== undefined && Object.keys(departmentType).length !== 0
            ?departmentType.map((d) => d.key)
            : "";
        const userIdSelected = 
          searchData.SEARCH_USER_ID === undefined
            ? ""
            : searchData.SEARCH_USER_ID.trim();
       
        http
          .get(
            "api/user/GetAllUsers?_userID=" +
              userIdSelected +
              "&_role=" +
              roletypeSelected +
              "&_department=" +
              departmenttypeSelected +
              "&_sysUser=" +
              userId,
              { timeout: 10000000}
          )
          .then((response) => {
            if (response.data.length > 0) {
              response.data.forEach((data) => {
                listItems.push({
                  key: data.USER_ID,
                  USER_ID: data.USER_ID,
                  USER_NAME: data.USER_NAME,
                  EMAIL: data.EMAIL,
                  USERROLE_ID: data.USERROLE_ID,
                  DEPT_ID: data.DEPT_ID,
                  USER_CATEGORY: data.USER_CATEGORY,
                  USER_DEPT_CATEGORY: data.USER_DEPT_CATEGORY,
                  ROLE_ID: data.ROLE_ID,
                  ROLE_NAME: data.ROLE_NAME,
                  DEPARTMENT_ID: data.DEPARTMENT_ID,
                  DEPARTMENT_NAME: data.DEPARTMENT_NAME,
                  LAST_ACCESS_DATETIME: data.LAST_ACCESS_DATETIME,
                  STATUS: data.ACTIVE_FLAG === "Y" ? "Active" : "Inactive",
                  UPDATE_ID: data.UPDATE_ID,
                  UPDATE_DATETIME: data.UPDATE_DATETIME,
                });
              });
            }
            setAllData(listItems);
          })
          .catch((err) => {
            toast.error("Error on getting user. Please try again. (send)");
            common.c_LogWebError(PAGE_NAME, functionName, err);
          })
          .finally(() => {
            setLoading(false);
            setLoadingText("Loading....");
          });
      } catch (err) {
        toast.error("Error on getting user. Please try again. (Received)");
        common.c_LogWebError(PAGE_NAME, functionName, err);
      }
    }
  }, [reload, actionAuthority, userId]);

  useEffect(() => {
    if (actionAuthority.READ === "Y") {
      let functionName = "";
      try {
        functionName = "use Effect Get Role List";
        setLoading(true);
        setLoadingText("Getting User Role List, Please wait....");

        http
          .get("api/user/GetRoleListFilter?_sysUser=" + userId, { timeout: 10000})
          .then((response) => {
            console.log("Get data from API")
            const listItems = [];
            for (var i = 0; i < response.data.length; i++) {
              listItems.push({
                key: response.data[i].ROLE_ID,
                value: response.data[i].ROLE_ID,
                label: response.data[i].ROLE_NAME,
              });
            }
            setRoleTypeData(listItems);
          })
          .catch((err) => {
            console.log("Send request")
            toast.error("Error on getting user role list. PLease try again. (Send)");
            common.c_LogWebError(PAGE_NAME, functionName, err);
          })
          .finally(() => {
            setLoading(false);
            setLoadingText("Loading....");
          });
      } catch (err) {
        console.log("Cannot get user role. Loading")
        toast.error("Error on getting user role list. Please try again. (Received)");
        common.c_LogWebError(PAGE_NAME, functionName, err);
      }
    }
  }, [reload, actionAuthority, userId]);

  //#region Event Listener
  const showHideRegisterModal = () => {
    setShowAdd(!showAdd);
  };

  const onReloadHandler = () => {
    setReload(!reload);
  };

  const onLoadingHandler = (load, text) => {
    setLoading(load);
    setLoadingText(text);
  };
  //#endregion

  //#region Export Excel
  const onExcelHandler = () => {
    const roletypeSelected =
      roleType !== undefined && Object.keys(roleType).length !== 0
        ? roleType.map((r) => r.key)
        : "";
    const userIdSelected =
      searchData.SEARCH_USER_ID === undefined
        ? ""
        : searchData.SEARCH_USER_ID.trim();

    window.open(
      common.c_getApiUrl() +
        "api/user/ExportExcel?_userId=" +
        userIdSelected +
        "&_role=" +
        roletypeSelected +
        "&_sysUser=" +
        userId
    );
  };
  //#endregion

  const inputUnrequiredHandler = (event) => {
    let functionName = "";

    try {
      functionName = inputUnrequiredHandler.name;
      const id = event.target.id;
      const val = event.target.value;

      setSearchData((prevState) => ({
        ...prevState,
        [id]: val,
      }));
    } catch (err) {
      common.c_LogWebError(PAGE_NAME, functionName, err);
    }
  };
  // CSS LINE
  const ColoredLine = ({ color }) => (
    <hr
      style={{
        color: color,
        backgroundColor: color,
        height: 0.5,
      }}
    />
  );

  return (
    <>
      <Comp.Header>User Maintenance</Comp.Header>
      <tr>
          <td style={{ verticalAlign: "middle", width: "10%" }}>Search By Role :</td>
          {/* <td style={{ verticalAlign: "middle", width: "5%" }}>:</td> */}
          <td style={{ verticalAlign: "middle", width: "35%" }}>
            <Comp.Select
              options={roleTypeData}
              closeMenuOnSelect={true}
              isClearable={true}
              hideSelectedOptions={false}
              value={roleType}
              onChange={setRoleType}
              onSubmit={onReloadHandler}
              isMulti
            />
          </td>

          <td style={{ verticalAlign: "middle", width: "10%" }}>Search By User ID :</td>
          <td style={{ verticalAlign: "middle", width: "35%" }}>
            <Comp.Input
              id="SEARCH_USER_ID"
              type="text"
              className="form-control"
              isSearch={true}
              value={searchData.SEARCH_USER_ID}
              onChange={inputUnrequiredHandler}
              onSubmit={onReloadHandler}
            />
          </td>
        </tr>
        <tr>
        <td colSpan="4">
            <div style={{ display: "flex", justifyContent: "flex-end", marginTop: "10px" }}>
              <Comp.Button id="btnSearch" type="general" onClick={onReloadHandler}>
                SEARCH
              </Comp.Button>
            </div>
          </td>
        </tr>

      {/* TABLE COLUMNS */}
      <div style={{backgroundColor: '#A52A2A'}}>
        <div className={`${CssClass.cardTwo} ${CssClass.tableCardTwo}`}>
          <h3 className={CssClass.tableTitle}>
          User List
          </h3>
          <ColoredLine color="black" />

        <div style={{display: 'flex', justifyContent: 'end'}} className={CssClass.btnWrapperTwo}>
          {actionAuthority.WRITE === "Y" && (
            <Comp.Button
              id="btnRegister"
              type="general"
              style={{width: 'auto'}}
              onClick={showHideRegisterModal}
            >
              ADD USER
              <IoIosPersonAdd className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button>
          )}
            <Comp.Button id="btnExcel" type="excel" onClick={onExcelHandler}>
              EXPORT
              <TbTableExport className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button>
        </div>
        <br></br>
        <UserList
          data={allData}
          onReload={onReloadHandler}
          page={PAGE_NAME}
          module={MODULE_ID}
          onLoading={onLoadingHandler}
          actionAuthority={actionAuthority}
        />
        </div>
        
        {showAdd && (
          <RegisterUser
            showHide={showAdd}
            page={PAGE_NAME}
            module={MODULE_ID}
            onHide={showHideRegisterModal}
            onReload={onReloadHandler}
            onLoading={onLoadingHandler}
          />
        )}

        {loading && <Comp.Loading>{loadingText}</Comp.Loading>}
        <Comp.AlertPopup />
      </div>
    </>
    
  );
};

export default UserHomePage;

