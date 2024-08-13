import { useState, useEffect } from "react";
import { useCookies } from "react-cookie";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { GetPageAuthorityFn } from "../../Common/authority";
import http from "../../Common/http-common";
import * as common from "../../Common/common";
import * as Comp from "../../Common/CommonComponents";
import CssClass from "../../Styles/common.module.css";
import CustomerList from "./CustomerManage/CustomerList";
import CustomerRegister from "./CustomerManage/CustomerRegister";
import { IoIosPersonAdd } from "react-icons/io";
import { TbTableExport } from "react-icons/tb";

const PAGE_NAME = "CustomerHomePage.js_";
const MODULE_ID = "WEB_CS_001";

const CustomerHomePage = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const navigate = useNavigate();

  const [allData, setAllData] = useState([]);
  const [loading, setLoading] = useState([]);
  const [loadingText, setLoadingText] = useState("Loading....");
  const [reload, setReload] = useState(false);

  const [showAdd, setShowAdd] = useState(false);

  // const [roleType, setRoleType] = useState([]);
  // const [roleTypeData, setRoleTypeData]= useState([]);
  const [searchData, setSearchData] = useState([]);

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

      try {
        functionName = "useEffect Get All Customers";

        setLoading(true);
        setLoadingText("Getting Customers, please wait...");

        const listItems = [];
        // const roletypeSelected = 
        //   roleType !== undefined && Object.keys(roleType).length !== 0
        //     ?roleType.map((r) => r.key)
        //     : "";
        const customerIdSelected = 
          searchData.SEARCH_CUSTOMER_ID === undefined
            ? ""
            : searchData.SEARCH_CUSTOMER_NAME.trim();
        const customerNameSelected = 
          searchData.SEARCH_CUSTOMER_NAME === undefined
            ? ""
            : searchData.SEARCH_CUSTOMER_NAME.trim();

        http
          .get(
            "api/customer/GetAllCustomers?_customerID=" +
              customerIdSelected +
              "&_customerName=" +
              customerNameSelected +
              // "&_role=" +
              // roletypeSelected +
              "&_sysUser=" +
              userId,
            { timeout: 10000000 }
          )
          .then((response) => {
            if (response.data.length > 0) {
              response.data.forEach((data) => {
                listItems.push({
                  key: data.CUSTOMER_ID,
                  CUSTOMER_ID: data.CUSTOMER_ID,
                  CUSTOMER_NAME: data.CUSTOMER_NAME,
                  EMAIL: data.EMAIL,
                  USERROLE_ID: data.USERROLE_ID,
                  ROLE_ID: data.ROLE_ID,
                  ROLE_NAME: data.ROLE_NAME,
                  PHONE_NO: data.PHONE_NO,
                  ADDRESS: data.ADDRESS,
                  COMPANY_NAME: data.COMPANY_NAME,
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
            toast.error("Error on getting customers. Please try again. (send)");
            common.c_LogWebError(PAGE_NAME, functionName, err);
          })
          .finally(() => {
            setLoading(false);
            setLoadingText("Loading....");
          });
      } catch (err) {
        toast.error("Error on getting customers. Please try again. (Received)");
        common.c_LogWebError(PAGE_NAME, functionName, err);
      }
    }
  }, [reload, actionAuthority, userId]);

  // useEffect(() => {
  //   if (actionAuthority.READ === "Y") {
  //     let functionName = "";
  //     try {
  //       functionName = "useEffect Get Role List";
  //       setLoading(true);
  //       setLoadingText("Getting Customer Role List, Please wait....");

  //       http
  //         .get("api/customer/GetRoleListFilter?_sysUser=" + userId, { timeout: 10000 })
  //         .then((response) => {
  //           console.log("Get data from API");
  //           const listItems = [];
  //           for (var i = 0; i < response.data.length; i++) {
  //             listItems.push({
  //               key: response.data[i].ROLE_ID,
  //               value: response.data[i].ROLE_ID,
  //               label: response.data[i].ROLE_NAME,
  //             });
  //           }
  //           setRoleTypeData(listItems);
  //         })
  //         .catch((err) => {
  //           console.log("Send request");
  //           toast.error("Error on getting role list. Please try again. (Send)");
  //           common.c_LogWebError(PAGE_NAME, functionName, err);
  //         })
  //         .finally(() => {
  //           setLoading(false);
  //           setLoadingText("Loading....");
  //         });
  //     } catch (err) {
  //       console.log("Cannot get user role. Loading");
  //       toast.error("Error on getting  user role list. Please try again. (Received)");
  //       common.c_LogWebError(PAGE_NAME, functionName, err);
  //     }
  //   }
  // }, [reload, actionAuthority, userId]);

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
    // const roletypeSelected =
    //   roleType !== undefined && Object.keys(roleType).length !== 0
    //     ? roleType.map((r) => r.key)
    //     : "";
    const customerIdSelected =
      searchData.SEARCH_CUSTOMER_ID === undefined
        ? ""
        : searchData.SEARCH_CUSTOMER_ID.trim();
    const customerNameSelected =
      searchData.SEARCH_CUSTOMER_NAME === undefined
        ? ""
        : searchData.SEARCH_CUSTOMER_NAME.trim();

    window.open(
      common.c_getApiUrl() +
        "api/customer/ExportExcel?_customerId=" +
        customerIdSelected +
        "&_customerName=" +
        customerNameSelected +
        // "&_role=" +
        // roletypeSelected +
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
      <Comp.Header>Customer Maintenance</Comp.Header>
      <tr>
      <td style={{ verticalAlign: "middle", width: "20%" }}>Customer Name :</td>
          <td style={{ verticalAlign: "middle", width: "40%" }}>
            <Comp.Input
              id="SEARCH_CUSTOMER_NAME"
              type="text"
              className="form-control"
              isSearch={true}
              value={searchData.SEARCH_CUSTOMER_NAME}
              onChange={inputUnrequiredHandler}
              onSubmit={onReloadHandler}
            />
          </td>
          <td style={{ verticalAlign: "middle", width: "10%" }}></td>
          {/* <td style={{ verticalAlign: "middle", width: "25%" }}>
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
          </td> */}
          <td colSpan="4">
            <div style={{ display: "flex", justifyContent: "center", marginRight: "200px" }}>
              <Comp.Button id="btnSearch" type="general" onClick={onReloadHandler}>
                SEARCH
              </Comp.Button>
            </div>
          </td>
        </tr>

      <div style={{backgroundColor: '#A52A2A'}}>
        <div className={`${CssClass.cardTwo} ${CssClass.tableCardTwo}`}>
          <h3 className={CssClass.tableTitle}>
            Customer List
          </h3>
          <ColoredLine color="black" />

          <div style={{ display: 'flex', justifyContent: 'end' }} className={CssClass.btnWrapperTwo}>
            {actionAuthority.WRITE === "Y" && (
              <Comp.Button
                id="btnRegister"
                type="general"
                style={{ width: 'auto' }}
                onClick={showHideRegisterModal}
              >
                ADD CUSTOMER
                <IoIosPersonAdd className='icon' style={{ marginBottom: '5px', fontSize: '20px', marginLeft: '5px' }} />
              </Comp.Button>
            )}
              <Comp.Button id="btnExcel" type="excel" onClick={onExcelHandler}>
                EXPORT
                <TbTableExport className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
              </Comp.Button>
          </div>
          <br></br>
          <CustomerList
            data={allData}
            onReload={onReloadHandler}
            page={PAGE_NAME}
            module={MODULE_ID}
            onLoading={onLoadingHandler}
            actionAuthority={actionAuthority}
          />
        </div>
        
        {showAdd && (
          <CustomerRegister
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

export default CustomerHomePage;
