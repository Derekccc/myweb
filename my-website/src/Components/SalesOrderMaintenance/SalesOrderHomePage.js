import { useState, useEffect } from "react";
import { useCookies } from "react-cookie";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { GetPageAuthorityFn } from "../../Common/authority";
import http from "../../Common/http-common";
import * as common from "../../Common/common";
import * as Comp from "../../Common/CommonComponents";
import CssClass from "../../Styles/common.module.css";
import SalesOrderList from "./SalesOrderManage/SalesOrderList";
import AddSalesOrder from "./SalesOrderManage/AddSalesOrder";
import { BsFileEarmarkPlus } from "react-icons/bs";
import { TbTableExport } from "react-icons/tb";

const PAGE_NAME = "SalesOrderHomePage.js_";
const MODULE_ID = "WEB_CS_003";

const SalesOrderHomePage = (props) => {
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

      try {
        functionName = "useEffect Get All Sales Order";

        setLoading(true);
        setLoadingText("Getting Sales Order Listing, please wait....");

        setAllData([]);

        const listItems = [];
        const customerNameSelected = 
          searchData.SEARCH_CUSTOMER_NAME === undefined
          ? "" : searchData.SEARCH_CUSTOMER_NAME.trim();
        const orderStatusSelected = 
          searchData.SEARCH_ORDER_STATUS === undefined
          ? "" : searchData.SEARCH_ORDER_STATUS.trim();

        http
          .get(`api/salesOrder/GetAllSalesOrder?_sysUser=${userId}`
              + "&_customerName=" + customerNameSelected
              + "&_orderStatus=" + orderStatusSelected,
             { timeout: 10000000 })
          .then((response) => {
            if (response.data.length > 0) {
              response.data.forEach((data) => {
                listItems.push({
                  key: data.SALES_ORDER_ID,
                  SALES_ORDER_ID: data.SALES_ORDER_ID,
                  CUSTOMER_NAME: data.CUSTOMER_NAME,
                  TOTAL_AMOUNT: `RM ${parseFloat(data.TOTAL_AMOUNT).toFixed(2)}`,
                  ORDER_DATETIME: data.ORDER_DATETIME,
                  SALES_ORDER_ACCEPT_FLAG: data.SALES_ORDER_ACCEPT_FLAG,
                  REVIEW: data.SALES_ORDER_ACCEPT_FLAG === "Y" ? "Accept" : "Reject",
                  ORDER_STATUS: data.ORDER_STATUS,
                  UPDATE_DATETIME: data.UPDATE_DATETIME,
                  UPDATE_ID: data.UPDATE_ID,
                });
              });
            }
            setAllData(listItems);
          })
          .catch((err) => {
            toast.error("Error on getting sales order list. Please try again. Send");
            common.c_LogWebError(PAGE_NAME, functionName, err);
          })
          .finally(() => {
            setLoading(false);
            setLoadingText("Loading...");
          });
        } catch(err) {
          toast.error("Error on getting sales order list. Please try again.");
          common.c_LogWebError(PAGE_NAME, functionName, err);
        }
    }
  }, [reload, actionAuthority, userId]);

  //#region Event Handle
  const showHideAddHandler = () => {
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
    const customerNameSelected =
      searchData.SEARCH_CUSTOMER_NAME === undefined
        ? ""
        : searchData.SEARCH_CUSTOMER_NAME.trim();
    const orderStatusSelected =
      searchData.SEARCH_ORDER_STATUS === undefined
        ? ""
        : searchData.SEARCH_ORDER_STATUS.trim();

    window.open(
      common.c_getApiUrl() + "api/salesOrder/ExportExcel?_sysUser=" + userId
        + "&_customerName=" + customerNameSelected 
        + "&_orderStatus=" + orderStatusSelected
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
    <Comp.Header>Order Maintenance</Comp.Header>
    <tr>
      <td style={{ verticalAlign: "middle", width: "10%" }}>Customer Name :</td>
          <td style={{ verticalAlign: "middle", width: "35%" }}>
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
          <td style={{ verticalAlign: "middle", width: "10%" }}>Order Status :</td>

          <td style={{ verticalAlign: "middle", width: "35%" }}>
            <Comp.Input
                id="SEARCH_ORDER_STATUS"
                type="text"
                className="form-control"
                isSearch={true}
                value={searchData.SEARCH_ORDER_STATUS}
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

    <div style={{ backgroundColor: '#A52A2A' }}>
        <div className={`${CssClass.cardTwo} ${CssClass.tableCardTwo}`}>
          <h3 className={CssClass.tableTitle}>
            Sales Order List
          </h3>
          <ColoredLine color="black" />
          
          <div style={{ display: 'flex', justifyContent: 'end' }}>
            {actionAuthority.WRITE === "Y" && (
              <Comp.Button
                id="btnAdd"
                type="general"
                style={{ width: 'auto' }}
                onClick={showHideAddHandler}
                className={CssClass.registerButton}
              >
                ADD ORDER
                <BsFileEarmarkPlus className='icon' style={{ marginBottom: '5px', fontSize: '18px', marginLeft: '5px' }} />
              </Comp.Button>
            )}
            <Comp.Button id="btnExcel" type="excel" onClick={onExcelHandler}>
              EXPORT
              <TbTableExport className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button>
          </div>
          <br></br>
          <SalesOrderList
            data={allData}
            onReload={onReloadHandler}
            page={PAGE_NAME}
            module={MODULE_ID}
            onLoading={onLoadingHandler}
            actionAuthority={actionAuthority}
          />
        </div>

        {showAdd && (
          <AddSalesOrder
            showHide={showAdd}
            page={PAGE_NAME}
            module={MODULE_ID}
            onHide={showHideAddHandler}
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

export default SalesOrderHomePage;
