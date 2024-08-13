import { useState, useEffect } from "react";
import { useCookies } from "react-cookie";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { GetPageAuthorityFn } from "../../Common/authority";
import http from "../../Common/http-common";
import * as common from "../../Common/common";
import * as Comp from "../../Common/CommonComponents";
import CssClass from "../../Styles/common.module.css";
import ProductList from "./ProductManage/ProductList";
import ProductRegister from "./ProductManage/ProductRegister";
import { SiMaterialformkdocs } from "react-icons/si";
import { TbTableExport } from "react-icons/tb";

const PAGE_NAME = "ProductHomePage.js_";
const MODULE_ID = "WEB_CS_002";

const ProductHomePage = (props) => {
  const navigate = useNavigate();
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);

  const [allData, setAllData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [loadingText, setLoadingText] = useState("Loading...");
  const [reload, setReload] = useState(false);

  const [showAdd, setShowAdd] = useState(false);
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
        functionName = "useEffect Get All Products";

        setLoading(true);
        setLoadingText("Getting Products Listing, please wait....");

        setAllData([]);

        const listItems = [];
        const productNameSelected = 
          searchData.SEARCH_PRODUCT_NAME === undefined
          ? "" : searchData.SEARCH_PRODUCT_NAME.trim();

        http
          .get(`api/product/GetAllProducts?_sysUser=${userId}`+ 
            "&_productName=" +
            productNameSelected,
            { timeout: 10000000 }
          )
          .then((response) => {
            if (response.data.length > 0) {
              response.data.forEach((data) => {
                listItems.push({
                  key: data.PRODUCT_ID,
                  PRODUCT_ID: data.PRODUCT_ID,
                  PRODUCT_NAME: data.PRODUCT_NAME,
                  PRODUCT_DESC: data.PRODUCT_DESC,
                  UNIT_COST: ` ${parseFloat(data.UNIT_COST).toFixed(2)}`,
                  UNIT_SELLING_PRICE: ` ${parseFloat(data.UNIT_SELLING_PRICE).toFixed(2)}`,
                  QUANTITY: parseInt(data.QUANTITY),
                  PRODUCT_ACTIVE_FLAG: data.PRODUCT_ACTIVE_FLAG,
                  STATUS: data.PRODUCT_ACTIVE_FLAG === "Y" ? "Active" : "Inactive",
                  UPDATE_DATETIME: data.UPDATE_DATETIME,
                  UPDATE_ID: data.UPDATE_ID,
                });
              });
            }
            setAllData(listItems);
          })
          .catch((err) => {
            toast.error("Error on getting user product list. Please try again.");
            common.c_LogWebError(PAGE_NAME, functionName, err);
          })
          .finally(() => {
            setLoading(false);
            setLoadingText("Loading...");
          });
        } catch(err) {
          toast.error("Error on getting user product list. Please try again.");
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

    const productIdSelected = 
          searchData.SEARCH_PRODUCT_ID === undefined
          ? "" : searchData.SEARCH_PRODUCT_ID.trim();
    const productNameSelected = 
          searchData.SEARCH_PRODUCT_NAME === undefined
          ? "" : searchData.SEARCH_PRODUCT_NAME.trim();

    window.open(
      common.c_getApiUrl() + 
        "api/product/ExportExcel?_sysUser=" + userId +
        "&_productName=" + productNameSelected +
        "&_productId=" + productIdSelected
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
    <Comp.Header>Product Maintenance</Comp.Header>
      <tr>
      <td style={{ verticalAlign: "middle", width: "20%" }}>Product Name :</td>
      &nbsp;&nbsp;&nbsp;
          <td style={{ verticalAlign: "middle", width: "40%" }}>
            <Comp.Input
              id="SEARCH_PRODUCT_NAME"
              type="text"
              className="form-control"
              isSearch={true}
              value={searchData.SEARCH_PRODUCT_NAME}
              onChange={inputUnrequiredHandler}
              onSubmit={onReloadHandler}
            />
          </td>
          <td style={{ verticalAlign: "middle", width: "10%" }}></td>
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
          Product List
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
              + PRODUCT
              <SiMaterialformkdocs className='icon' style={{marginBottom: '5px', fontSize: '18px', marginLeft: '5px'}}/>
            </Comp.Button>
           
          )}
            <Comp.Button id="btnExcel" type="excel" onClick={onExcelHandler}>
              EXPORT
              <TbTableExport className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button>
           </div>
        <br></br>
        <ProductList
          data={allData}
          onReload={onReloadHandler}
          page={PAGE_NAME}
          module={MODULE_ID}
          onLoading={onLoadingHandler}
          actionAuthority={actionAuthority}
        />
      </div>

      {showAdd && (
        <ProductRegister
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

export default ProductHomePage;
  
