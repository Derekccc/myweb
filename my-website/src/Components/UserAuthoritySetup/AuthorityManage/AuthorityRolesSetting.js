import { useState, useEffect } from "react";
import { useCookies } from "react-cookie";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import http from "../../../Common/http-common";
import * as common from "../../../Common/common";
import * as Comp from "../../../Common/CommonComponents";
import { GetPageAuthorityFn } from "../../../Common/authority";
import CssClass from "../../../Styles/common.module.css";
import { Accordion } from "react-bootstrap";
import RAUserInfoTable from "../../UserTabletSetup/RoleAuthorityMaintenance/RAUserInfoTable";
import RAConsumerInfoTable from "../../UserTabletSetup/RoleAuthorityMaintenance/RAConsumerInfoTable";

const PAGE_NAME = "AuthorityRolesSetting.js_";
const MODULE_ID = "WEB_UT_004";

const AuthorityRolesSetting = (props) => {
  document.title = common.c_TITLE + " - Authority Roles Setting";

  const navigate = useNavigate();
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);

  const [roleOptions, setRoleOptions] = useState([]);
  const [selectedRole, setSelectedRole] = useState(null);
  const [allData, setAllData] = useState({
    CAT_USER_TABLET_SETUP: [],
    CAT_SYSTEM_SETUP: [],
    CAT_BASIC_SETUP: [],
    CAT_DEVICE_SETUP: [],
    TABLET_BUTTON: [],
    TABLET_FEATURE: [],
    CAT_ASS_FEA_AUTH: [],
    CAT_WIN_FEA_AUTH: [],
    CAT_CONSUMER_TABLET_SETUP: [],
  });

  const [loading, setLoading] = useState(false);
  const [loadingText, setLoadingText] = useState("Loading...");

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
    let functionName = "";
    const roleDetail = [];

    try {
      functionName = "useEffect Get All Roles";
      onLoadingHandler(true, "Getting User Role List, please wait...");

      http
        .get("api/roleAuthority/GetAllRole?_sysUser=" + userId, {
          timeout: 10000000,
        })
        .then((response) => {
          if (response.data.length > 0) {
            response.data.forEach((data, counter) => {
              roleDetail.push({
                key: data.ROLE_ID,
                value: data.ROLE_ID,
                label: data.ROLE_NAME,
              });
            });
          }
          setRoleOptions(roleDetail);
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
      toast.error("Error on getting user role list. Please try again.");
      common.c_LogWebError(PAGE_NAME, functionName, err);
    }
  }, [userId]);

// #region Event Handler
const onLoadingHandler = (load, text) => {
  setLoading(load);
  setLoadingText(text);
};
//#endregion




const selectRoleHandler = (val) => {
  if (val !== selectedRole) {
    let functionName = "";
    try {
      functionName = selectRoleHandler.name;
      setSelectedRole(val);
      GetFeatureAuthority(val.key, "CAT_SYSTEM_SETUP")
        .then(() => GetFeatureAuthority(val.key, "CAT_USER_TABLET_SETUP"))
        .then(() => GetFeatureAuthority(val.key, "CAT_CONSUMER_TABLET_SETUP"))
        .then(() => GetFeatureAuthority(val.key, "CAT_BASIC_SETUP"))
        .then(() => GetFeatureAuthority(val.key, "CAT_DEVICE_SETUP"))
        .then(() => GetFeatureAuthority(val.key, "CAT_TIGHTENING_SETUP"))
        .then(() => GetFeatureAuthority(val.key, "CAT_TIGHTENING_INFO"))
        .then(() => GetFeatureAuthority(val.key, "CAT_PLAN"))
        .then(() => GetFeatureAuthority(val.key, "CAT_SAP_MGMT"))
        .then(() => GetFeatureAuthority(val.key, "CAT_VEHICLE_INFO"))
      // GetTabletAuthority(val.key);
      GetTabletButtonAuthority(val.key);
      GetTabletFeatureAuthority(val.key);
    } catch (err) {
      common.c_LogWebError(PAGE_NAME, functionName, err);
    }
  }
};


async function GetFeatureAuthority(roleId, moduleCat) {
  let functionName2 = "";
  const listItems = [];
  try {
    functionName2 = PAGE_NAME + GetFeatureAuthority.name;
    onLoadingHandler(
      true,
      `Getting ${moduleCat} feature authority list, please wait...`
    );

    await http
      .get(
        `api/roleAuthority/GetFeatureAuthority?_sysUser=${userId}&_roleId=${roleId}&_moduleCat=${moduleCat}`,
        { timeout: 10000000 }
      )
      .then((response) => {
        if (response.data.length > 0) {
          response.data.forEach((data, counter) => {
            var details = [];
            data.AUTHORITY_DETAILS.forEach((detail, i) => {
              details.push({
                key: detail.FEATURE_ID,
                rowNo: i,
                FEATURE_ID: detail.FEATURE_ID,
                FEATURE_TYPE: detail.FEATURE_TYPE,
                ROLE_AUTHORITY_ID: detail.ROLE_AUTHORITY_ID,
                AUTHORIZED: detail.AUTHORIZED,
              });
            });
            listItems.push({
              key: data.MODULE_NAME,
              rowNo: counter,
              MODULE_NAME: data.MODULE_NAME,
              AUTHORITY_DETAILS: details,
            });
          });
        }
        setAllData((prevState) => ({
          ...prevState,
          [moduleCat]: listItems,
        }));
      })
      .catch((err) => {
        toast.error(
          `Error on getting ${moduleCat} feature authority list. Please try again.`
        );
        common.c_LogWebError(PAGE_NAME, functionName2, err);
      })
      .finally(() => {
        onLoadingHandler(false, "Loading...");
      });
  } catch (err) {
    onLoadingHandler(false, "Loading...");
    toast.error(
      `Failed to get ${moduleCat} feature authority list. Please try again.`
    );
    common.c_LogWebError(PAGE_NAME, functionName2, err);
  }
}

// function GetTabletAuthority(roleId) {
//   let functionName2 = "";
//   const listItems = [];
//   try {
//     functionName2 = PAGE_NAME + GetTabletAuthority.name;
//     onLoadingHandler(true, `Getting tablet authority list, please wait...`);

//     http
//       .get(
//         `api/roleAuthority/GetTabletAuthority?_sysUser=${userId}&_roleId=${roleId}`,
//         { timeout: 10000000 }
//       )
//       .then((response) => {
//         if (response.data.length > 0) {
//           response.data.forEach((data, counter) => {
//             listItems.push({
//               key: data.DEVICE_MST_ID,
//               rowNo: counter,
//               DEVICE_MST_ID: data.DEVICE_MST_ID,
//               DEVICE_DESC: data.DEVICE_DESC,
//               ROLE_AUTHORITY_ID: data.ROLE_AUTHORITY_ID,
//               AUTHORIZED: data.AUTHORIZED,
//             });
//           });
//         }
//         setAllData((prevState) => ({
//           ...prevState,
//           DEVICE_SETUP: listItems,
//         }));
//       })
//       .catch((err) => {
//         toast.error(
//           `Error on getting tablet authority list. Please try again.`
//         );
//         common.c_LogWebError(PAGE_NAME, functionName2, err);
//       })
//       .finally(() => {
//         onLoadingHandler(false, "Loading...");
//       });
//   } catch (err) {
//     onLoadingHandler(false, "Loading...");
//     toast.error(`Failed to get tablet authority list. Please try again.`);
//     common.c_LogWebError(PAGE_NAME, functionName2, err);
//   }
// }


function GetTabletButtonAuthority(roleId) {
  let functionName2 = "";
  const listItems = [];
  try {
    functionName2 = PAGE_NAME + GetTabletButtonAuthority.name;
    onLoadingHandler(
      true,
      `Getting tablet button authority list, please wait...`
    );

    http
      .get(
        `api/roleAuthority/GetTabletButtonAuthority?_sysUser=${userId}&_roleId=${roleId}`,
        { timeout: 10000000 }
      )
      .then((response) => {
        if (response.data.length > 0) {
          response.data.forEach((data, counter) => {
            listItems.push({
              key: data.FEATURE_ID,
              rowNo: counter,
              FEATURE_ID: data.FEATURE_ID,
              MODULE_NAME: data.MODULE_NAME,
              ROLE_AUTHORITY_ID: data.ROLE_AUTHORITY_ID,
              AUTHORIZED: data.AUTHORIZED,
            });
          });
        }
        setAllData((prevState) => ({
          ...prevState,
          TABLET_BUTTON: listItems,
        }));
      })
      .catch((err) => {
        toast.error(
          `Error on getting tablet button authority list. Please try again.`
        );
        common.c_LogWebError(PAGE_NAME, functionName2, err);
      })
      .finally(() => {
        onLoadingHandler(false, "Loading...");
      });
  } catch (err) {
    onLoadingHandler(false, "Loading...");
    toast.error(
      `Failed to get tablet button authority list. Please try again.`
    );
    common.c_LogWebError(PAGE_NAME, functionName2, err);
  }
}

function GetTabletFeatureAuthority(roleId) {
  let functionName2 = "";
  const listItems = [];
  try {
    functionName2 = PAGE_NAME + GetTabletFeatureAuthority.name;
    onLoadingHandler(
      true,
      `Getting tablet feature authority list, please wait...`
    );

    http
      .get(
        `api/roleAuthority/GetTabletFeatureAuthority?_sysUser=${userId}&_roleId=${roleId}`,
        { timeout: 10000000 }
      )
      .then((response) => {
        if (response.data.length > 0) {
          response.data.forEach((data, counter) => {
            listItems.push({
              key: data.FEATURE_ID,
              rowNo: counter,
              FEATURE_ID: data.FEATURE_ID,
              MODULE_NAME: data.MODULE_NAME,
              ROLE_AUTHORITY_ID: data.ROLE_AUTHORITY_ID,
              AUTHORIZED: data.AUTHORIZED,
            });
          });
        }
        setAllData((prevState) => ({
          ...prevState,
          TABLET_FEATURE: listItems,
        }));
      })
      .catch((err) => {
        toast.error(
          `Error on getting tablet feature authority list. Please try again.`
        );
        common.c_LogWebError(PAGE_NAME, functionName2, err);
      })
      .finally(() => {
        onLoadingHandler(false, "Loading...");
      });
  } catch (err) {
    onLoadingHandler(false, "Loading...");
    toast.error(
      `Failed to get tablet feature authority list. Please try again.`
    );
    common.c_LogWebError(PAGE_NAME, functionName2, err);
  }
}

function ChangeFeatureAuthority(featureId, val) {
  let functionName = "";
  try {
    functionName = PAGE_NAME + ChangeFeatureAuthority.name;
    onLoadingHandler(true, "Changing authority, please wait...");
    const data = {
      ROLE_ID: selectedRole.key,
      ROLE_NAME: selectedRole.label,
      FEATURE_ID: featureId,
      AUTHORIZED: val === true ? "Y" : "N",
      UPDATE_ID: userId,
      FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: MODULE_ID },
    };
    http
      .post("api/roleAuthority/ChangeFeatureAuthority", data, {
        timeout: 10000,
      })
      .then((response) => {
        toast.success(
          `Authority is successfully ${val === true ? "inserted" : "removed"
          }.`
        );
      })
      .catch((err) => {
        toast.error(
          `Failed to ${val === true ? "insert" : "remove"
          } authority. Please try again.`
        );
        common.c_LogWebError(PAGE_NAME, functionName, err);
      })
      .finally(() => {
        onLoadingHandler(false, "Loading...");
      });
  } catch (err) {
    onLoadingHandler(false, "Loading...");
    toast.error(
      `Failed to ${val === true ? "insert" : "remove"
      } authority. Please try again.`
    );
    common.c_LogWebError(PAGE_NAME, functionName, err);
  }
}

  return (
    <>
      <Comp.Header>Permission Management</Comp.Header>
      <div style={{ margin: "10px" }}>
        <div style={{ padding: "0 10px" }}>
          <table className="table table-bordered table-hover">
            <tbody>
              <tr>
                <td style={{ verticalAlign: "middle", width: "20%" }}>Select Role:</td>
                <td style={{ verticalAlign: "middle", width: "5%" }}>:</td>
                <td style={{ verticalAlign: "middle", width: "25%" }}>
                  <Comp.Select
                    options={roleOptions}
                    closeMenuOnSelect={true}
                    hideSelectedOptions={false}
                    allowSelectAll={false}
                    value={selectedRole}
                    onChange={selectRoleHandler}
                  />
                </td>
                <td style={{ verticalAlign: "middle", width: "20%" }}></td>
                <td style={{ verticalAlign: "middle", width: "5%" }}></td>
                <td style={{ verticalAlign: "middle", width: "25%" }}></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div className={`${CssClass.cardTwo} ${CssClass.tableCardTwo}`}>
      <Accordion style={{width: '100%'}}>
        <Accordion.Item eventKey="0">
            <Accordion.Header>
              <h5 style={{ fontWeight: "bold" }}>USER SETUP</h5>
            </Accordion.Header>
            
            <Accordion.Body>
              <div>
                <RAUserInfoTable
                  data={
                    Object.keys(allData).length !== 0 &&
                      allData.CAT_USER_TABLET_SETUP.length !== 0 &&
                      allData.CAT_USER_TABLET_SETUP[0].AUTHORITY_DETAILS
                        .length !== 0
                      ? allData.CAT_USER_TABLET_SETUP
                      : []
                  }
                  ChangeFeatureAuthority={ChangeFeatureAuthority}
                  actionAuthority={actionAuthority}
                />
              </div>
            </Accordion.Body>
          </Accordion.Item>
        </Accordion>
      </div>
      <div className={`${CssClass.cardTwo} ${CssClass.tableCardTwo}`}>
      <Accordion style={{width: '100%'}}>
        <Accordion.Item eventKey="0">
            <Accordion.Header>
              <h5 style={{ fontWeight: "bold" }}>CONSUMER SETUP</h5>
            </Accordion.Header>
            
            <Accordion.Body>
              <div>
                <RAConsumerInfoTable
                  data={
                    Object.keys(allData).length !== 0 &&
                      allData.CAT_CONSUMER_TABLET_SETUP.length !== 0 &&
                      allData.CAT_CONSUMER_TABLET_SETUP[0].AUTHORITY_DETAILS
                        .length !== 0
                      ? allData.CAT_CONSUMER_TABLET_SETUP
                      : []
                  }
                  ChangeFeatureAuthority={ChangeFeatureAuthority}
                  actionAuthority={actionAuthority}
                />
              </div>
            </Accordion.Body>
          </Accordion.Item>
        </Accordion>
      </div>
      {loading && <Comp.Loading>{loadingText}</Comp.Loading>}
      <Comp.AlertPopup />
    </>
  );
};

export default AuthorityRolesSetting;
