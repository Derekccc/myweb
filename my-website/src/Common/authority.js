import http from "./http-common";

const moduleList = {
  CAT_SYSTEM_SETUP: [
    "WEB_SYS_001", 	//System Configuration
    "WEB_SYS_002",	//Policy Configuration
    "WEB_SYS_003",	//Shipping Mark Configuration,
    "WEB_SYS_004",  //Printer Setup
    "WEB_SYS_005", //Generate Body QR Configuration
    "WEB_SYS_006", //Extract Body QR Configuration
  ],
  CAT_USER_TABLET_SETUP: [
    "WEB_UT_001", 	//User Maintenance
    "WEB_UT_002",		//User Role Maintenance
    // "WEB_UT_003", 	//Tablet Maintenance
    "WEB_UT_004",		//User Role Authority Maintenance
    "WEB_UT_005",   //User Department Authority maintenance
  ],
  CAT_CONSUMER_TABLET_SETUP: [
    "WEB_CS_001", 	//Customer Maintenance
    "WEB_CS_002",   //Product Maintenance
    "WEB_CS_003",   //Sales Order Maintenance
    "WEB_CS_004",   //Sales Chart maintenence
  ],
  CAT_BASIC_SETUP: [
    "WEB_BSC_001",		//Prod. Line Maintenance
    "WEB_BSC_002",		//Shipping Mark Country Maintenance
    "WEB_BSC_003",		//Station Maintenance
    "WEB_BSC_004",		//Station Gp Maintenance
    "WEB_BSC_005",		//Model Maintenance
    "WEB_BSC_006",		//Model Matl Gp Maintenance
    "WEB_BSC_007",		//Model Matl Gp Paint Maintenance
    "WEB_BSC_008",		//Model Matl Gp Shipping Mark Maintenance
    "WEB_BSC_009",		//Station Route Maintenance
    "WEB_BSC_010",		//Broadcast Maintenance
    "WEB_BSC_011",		//Defect By-Pass Maintenance
    "WEB_BSC_012",		//Station Reconfirm Maintenance
  ],
  CAT_DEVICE_SETUP: [
    "WEB_DVC_001",		//Tightening Maintenance Controller
    "WEB_DVC_002",    //Tightening Tool Maintenance
    "WEB_DVC_003",		//Touch Panel Maintenance 
  ],
  CAT_TIGHTENING_SETUP: [
    "WEB_TTS_001",		//Assembly Image Maintenance
    "WEB_TTS_002",		//Model Assembly Image Maintenance 
    "WEB_TTS_003",		//Rework Torque Reason Maintenance 
    "WEB_TTS_004",		//Suspend Cycle Time Reason Maintenance 
  ],
  CAT_TIGHTENING_INFO: [
    "WEB_TTI_001",		//Tightening Result
    "WEB_TTI_002",		//Tightening Result History 
  ],
  CAT_INSP_SETUP: [
    "WEB_INSP_001",	//Defect Maintenance
    "WEB_INSP_002",	//Defect Rank Maintenance
    "WEB_INSP_003",	//Defect Category Maintenance
    "WEB_INSP_004",	//Defect Part Maintenance
    "WEB_INSP_005",	//Defect Position Maintenance
    "WEB_INSP_006",	//Station Check List Maintenance
    "WEB_INSP_007",	//Insepction Sheet Img Maintenance
    "WEB_INSP_008",	//Specification Img Maintenance
    "WEB_INSP_009", //Inspection Sheet Img Defect
    "WEB_INSP_010", //Inspection Sheet Img Defect Position
    "WEB_INSP_011", //Insepction Sheet Defect Category & Part Maintenance
    "WEB_INSP_012", //Insepction Sheet Img Checkpoint Maintenance

  ],
  CAT_RWK_SETUP: [
    "WEB_RWK_001",	//Rework Action Maintenance
    "WEB_RWK_002",	//Defect Root Cause Maintenance
    "WEB_RWK_003",	//Part Removal Check List Maintenance
  ],
  CAT_PLAN: [
    "WEB_PLN_001",	//Time Scope
    "WEB_PLN_002",	//Time Pattern
    "WEB_PLN_003",	//Working Calendar
    "WEB_PLN_004",  //Plan Quantity
    "WEB_PLN_005",  //BodyProductionPlan
    "WEB_PLN_006",  //Model Assembly Cycle Time
  ],
  CAT_SAP_MGMT: [
    "WEB_SAP_MGMT_001",	//SAP Action Code Maintenance
    "WEB_SAP_MGMT_002",	//SAP Plan
    "WEB_SAP_MGMT_003",	//SAP Data Change
    "WEB_SAP_MGMT_004",	//SAP Data Change History
    "WEB_SAP_MGMT_005",	//SAP Data Control
    "WEB_SAP_MGMT_006",	//SAP Data Update
    "WEB_SAP_MGMT_007",	//SAP Data Update History
    "WEB_SAP_MGMT_008", //PPSAP Update
    "WEB_SAP_MGMT_009", //PPSAP Update History
    "WEB_SAP_MGMT_010", //PPSAP Status History
  ],
  CAT_VEHICLE_INFO: [
    "WEB_VEHICLE_INFO_001",	//Vehicle History
    "WEB_VEHICLE_INFO_002",	//Vehicle Trace
    //"WEB_VEHICLE_INFO_003",	//Vehicle History Body No
    "WEB_VEHICLE_INFO_004",	//Vehicle Cycle Time History
  ],
  CAT_INSP_INFO: [
    "WEB_INS_INFO_001",	// Defect & Rework List
    "WEB_INS_INFO_002",	// Defect Per Unit
    "WEB_INS_INFO_003", // Rework Time
    "WEB_INS_INFO_004", // Defect Graph
    "WEB_INS_INFO_005",	// Daily Defect Per Unit
    "WEB_INS_INFO_006", // Check List History
  ],
  CAT_PROD_OPERATION: [
    "WEB_PROD_OPE_001",  //Production Volume
    "WEB_PROD_OPE_002",  //Operation Ratio
  ],
  CAT_MONITOR: [
    "WEB_MNT_001",  //Achievement TIB
    "WEB_MNT_002",  //WIP TIB
    "WEB_MNT_003",  //Achievement History TIB
    "WEB_MNT_004",  //Paint Shop & PBS TIB
    "WEB_MNT_005",  //TIB Assembly Performance
  ],
  CAT_WIP_MGMT: [
    "WEB_WIP_001",	//WIP Control
    "WEB_WIP_002",	//By-Pass Registration
    "WEB_WIP_003",	//Shipping/Delivery Point
    "WEB_WIP_004",	//Shipping/Delivery Point History
    "WEB_WIP_005",  //Pack Month Configuration
    "WEB_WIP_006",  //WIP Report
    "WEB_WIP_007",  //WIP Summary
    "WEB_WIP_008",  //WIP History
    "WEB_WIP_009",  //Unmatch Body
    "WEB_WIP_010",  //Unmatch Body History
    "WEB_WIP_011",  //Assembly Data Change
    "WEB_WIP_012",  //Assembly Data Change History
  ],
  CAT_ASS_FEA_AUTH: [
    "ASS_FEA_001",	//Assembly - Reset Record
    "ASS_FEA_002",	//Assembly - Suspend & Resume Station
  ],
};

const moduleBinary = {
  0: 0b0000000000000001,
  1: 0b0000000000000010,
  2: 0b0000000000000100,
  3: 0b0000000000001000,
  4: 0b0000000000010000,
  5: 0b0000000000100000,
  6: 0b0000000001000000,
  7: 0b0000000010000000,
  8: 0b0000000100000000,
  9: 0b0000001000000000,
  10: 0b0000010000000000,
  11: 0b0000100000000000,
  12: 0b0001000000000000,
  13: 0b0010000000000000,
  14: 0b0100000000000000,
  15: 0b1000000000000000,
};

console.log("module list", moduleList);
function GetModuleList(moduleId) {
  return moduleList[moduleId];
}

function GetUserAuthorityFn(userId, moduleCat, moduleList) {
  let promise = new Promise(function (resolve, reject) {
    const listItems = [];
    let authority = 0b0000000000000000;
    console.log("module list", moduleList);
    try {
      http
        .get(
          `api/WebCommon/GetUserAuthority?_userId=${userId}&_moduleCat=${moduleCat}`,
          { timeout: 10000000 }
        )
        .then((response) => {
          if (response.data.length > 0) {
            response.data.forEach((data, counter) => {
              listItems.push({
                MODULE_ID: data.MODULE_ID,
              });
            });
            moduleList.forEach((m, i) => {
              var module = listItems.filter((d) => d.MODULE_ID === m);
              if (module.length > 0) {
                authority = authority | moduleBinary[i];
              }
            });
          }
          console.log(response);
          resolve(authority);
        })
        .catch((err) => {
          console.log("test", err);
          reject(`Error on getting user authority list.`);
        });
    } catch (err) {
      console.log("test", err);
      reject(`Error on getting user authority list.`);
    }
  });
  return promise;
}

function GetPageAuthorityFn(userId, moduleId) {
  let promise = new Promise(function (resolve, reject) {
    var listItems = [];
    try {
      http
        .get(
          `api/WebCommon/GetPageAuthority?_userId=${userId}&_moduleId=${moduleId}`,
          { timeout: 100000 }
        )
        .then((response) => {
          if (response.data.length > 0) {
            response.data.forEach((data, counter) => {
              listItems = {
                ...listItems,
                [data.FEATURE_TYPE]: data.AUTHORIZED,
              };
            });
          }
          resolve(listItems);
        })
        .catch((err) => {
          reject(`Error on checking user authority on ${moduleId}.`);
        });
    } catch (err) {
      reject(`Error on checking user authority on ${moduleId}.`);
    }
  });
  return promise;
}

export { moduleBinary, GetUserAuthorityFn, GetPageAuthorityFn, GetModuleList };
