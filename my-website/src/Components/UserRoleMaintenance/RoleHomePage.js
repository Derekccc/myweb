import { useState, useEffect } from "react";
import { useCookies } from "react-cookie";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { GetPageAuthorityFn } from "../../Common/authority";
import http from "../../Common/http-common";
import * as common from "../../Common/common";
import * as Comp from "../../Common/CommonComponents";
import CssClass from "../../Styles/common.module.css";
import RoleList from './RoleManage/RoleList';
import RegisterRole from './RoleManage/RegisterRole';
import { IoPersonAdd } from "react-icons/io5";
import { TbTableExport } from "react-icons/tb";

const PAGE_NAME = "RoleHomePage.js_";
const MODULE_ID = "WEB_UT_002";

const RoleHomePage = (props) => {
  const navigate = useNavigate();
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);

  const [allData, setAllData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [loadingText, setLoadingText] = useState("Loading...");
  const [reload, setReload] = useState(false);
  const [searchData, setSearchData] = useState([]);

  const [showAdd, setShowAdd] = useState(false);

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
        common.c_LogWebError(PAGE_NAME, "GetPageAuthorityFn", error);
        toast.error(error);
        navigate("/");
      }
    );
  }, [userId, navigate]);

  useEffect(() => {
    if (actionAuthority.READ === "Y") {
      let functionName = "";

      try{
        functionName = "useEffect Get All Roles";

        setLoading(true);
        setLoadingText("Getting Role List, please wait...");

        setAllData([]);

        const listItems = [];
        const roleNameSelected = 
          searchData.SEARCH_ROLE_NAME === undefined
            ? ""
            : searchData.SEARCH_ROLE_NAME.trim();


        http
          .get(`api/role/GetAllRoles?_sysUser=${userId}` +
              "&_roleName=" +  roleNameSelected,
              { timeout: 10000000 })
          .then((response) => {
            if (response.data.length > 0) {
              response.data.forEach((data) => {
                listItems.push({
                  key: data.ROLE_ID,
                  ROLE_ID: data.ROLE_ID,
                  ROLE_NAME: data.ROLE_NAME,
                  ROLE_DESC: data.ROLE_DESC,
                  ROLE_ACTIVE_FLAG: data.ROLE_ACTIVE_FLAG,
                  STATUS: data.ROLE_ACTIVE_FLAG === "Y" ? "Active" : "Inactive",
                  UPDATE_DATETIME: data.UPDATE_DATETIME,
                  UPDATE_ID: data.UPDATE_ID,
                });
              });
            }
            setAllData(listItems);
          })
          .catch((err) => {
            toast.error("Error on getting user role list. Please try again.");
            common.c_LogWebError(PAGE_NAME, functionName, err);
          })
          .finally(() => {
            setLoading(false);
            setLoadingText("Loading...");
          });
        } catch (err) {
          toast.error("Error on getting roles. Please try again. (Received)");
          common.c_LogWebError(PAGE_NAME, functionName, err);
        }
      }
    }, [reload, actionAuthority, userId]);

  //#region Event Handle
  const showHideRegisterHandler = () => {
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

    const roleNameSelected =
      searchData.SEARCH_ROLE_NAME === undefined
        ? ""
        : searchData.SEARCH_ROLE_NAME.trim();

    window.open(
      common.c_getApiUrl() + "api/role/ExportExcel?_sysUser=" + userId
      + "&_roleName=" + roleNameSelected
    );
  };
  //#endregion

  const inputUnrequiredHandler = (event) => {
    let functionName = "";

    try{
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
    <Comp.Header>Role Maintenance</Comp.Header>
    <tr>
      <td style={{ verticalAlign: "middle", width: "20%" }}> Role Name :</td>
      &nbsp;&nbsp;&nbsp;
        <td style={{ verticalAlign: "middle", width: "40%" }}>
          <Comp.Input
            id="SEARCH_ROLE_NAME"
            type="text"
            className="form-control"
            isSearch={true}
            value={searchData.SEARCH_ROLE_NAME}
            onChange={inputUnrequiredHandler}
            onSubmit={onReloadHandler}
          />
        </td>
      <td style={{ verticalAlign: "middle", width: "10%" }}></td>
      <td colSpan="4">
        <div style={{ display: "flex", justifyContent: "center", marginRight: "180px" }}>
          <Comp.Button id="btnSearch" type="general" onClick={onReloadHandler}>
            SEARCH
          </Comp.Button>
        </div>
      </td>
    </tr>

    <div style={{backgroundColor: '#A52A2A'}}>
      <div className={`${CssClass.cardTwo} ${CssClass.tableCardTwo}`}>
        <h3 className={CssClass.tableTitle}>
          Role List
        </h3>
        <ColoredLine color="black" />
         
          <div style={{display: 'flex', justifyContent: 'end'}}>
          {actionAuthority.WRITE === "Y" && (
            <Comp.Button
              id="btnRegister"
              type="general"
              style={{width: 'auto'}}
              onClick={showHideRegisterHandler}
              className={CssClass.registerButton}
            >
              ADD ROLE
              <IoPersonAdd className='icon' style={{marginBottom: '5px', fontSize: '18px', marginLeft: '5px'}}/>
            </Comp.Button>
           
          )}
            <Comp.Button id="btnExcel" type="excel" onClick={onExcelHandler}>
              EXPORT
              <TbTableExport className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button>
           </div>
        <br></br>
        <RoleList
          data={allData}
          onReload={onReloadHandler}
          page={PAGE_NAME}
          module={MODULE_ID}
          onLoading={onLoadingHandler}
          actionAuthority={actionAuthority}
        />
      </div>

      {showAdd && (
        <RegisterRole
          showHide={showAdd}
          page={PAGE_NAME}
          module={MODULE_ID}
          onHide={showHideRegisterHandler}
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

export default RoleHomePage;
