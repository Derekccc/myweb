import http from "./http-common";
import { c_WEB_IP } from "./commonURL";
import { c_API_IP } from "./commonURL";
import moment from "moment";

const pageName = "common.js";

/////// timeout range need to be further confirm
const c_TITLE = "Porsche VTQS";
const DEFAULT_TIMEOUT = 5000;
const ADD_EDIT_TIMEOUT = 100000;
const ACTIVATE_DELETE_TIMEOUT = 5000;
const GET_ALLDATA_TIMEOUT = 1000000;
const GET_OPTIONDATA_TIMEOUT = 10000;

const EMAIL_REGEX = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
const PASSWORD_REGEX = /^.{8,}$/;
const NUMBER_ALPHABET_REGEX = /^[A-Z0-9a-z]*$/;
const NUMBER_REGEX = /^[0-9]*$/;
const PHONE_NUMBER_REGEX = /^01\d-?\d{7,8}$/;
const SYMBOL_REGEX = /;~!@#$%^&*_-|'"<>/;
const FREETEXT_REGEX = /^[\w]*$/;

const c_getApiUrl = () => {
  return c_API_IP;
}

const c_getWebUrl = () => {
  return c_WEB_IP;
}

const c_getWebLoginUrl = () => {
  return c_WEB_IP;
}

const c_GetFunctionName = (_currentFunctionName) => {
  let functionName = "";

  try {
    functionName = _currentFunctionName;
    functionName = functionName.substring("function ".length);
    functionName = functionName.substring(0, functionName.indexOf("("));
  } catch (err) {
    c_LogWebError(pageName, "c_GetFunctionName()", err);
  }

  return functionName + "()";
};

const c_LogWebError = (_pageName, _functionName, err) => {
  try {
    let error = {
      PAGE_NAME: _pageName,
      FUNCTION_NAME: _functionName,
      MESSAGE: (err.response === undefined ? err.stack : (err.message + "_" + err.response.data.MESSAGE)),
    };
    const url = c_getApiUrl() + "api/WebCommon/LogWebError";

    http
      .post(url, error)
      .then((response) => { })
      .catch((err) => {
        console.log(err.response.statusText === "timeout" ? "Server response timeout" : err.response.statusText);
      });
  } catch (err) {
    console.log(err);
  }
};

const c_EncryptData = (password) => {
  var CryptoJS = require("crypto-js");
  var key = CryptoJS.enc.Utf8.parse("7061737323313233");
  var iv = CryptoJS.enc.Utf8.parse("7061737323313233");
  var encrypted = CryptoJS.AES.encrypt(password, key,
    {
      keySize: 128 / 8,
      iv: iv,
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7
    });
  return encrypted;
  
}

const c_Dis_DateTime = (_data) => {
  //Convert Datetime Display
  let functionName = "";

  try {
    functionName = c_Dis_DateTime.name; //Must at the first line in try block

    let getDatetime = moment(_data, "YYYY-MM-DDTHH:mm:ss", true);
    let dis_Datetime = getDatetime.format("DD/MM/YYYY hh:mm:ss A");
    let disDay_Datetime = getDatetime.format("DD/MM/YYYY");
    let disDay_Datetime_YYYYMMDD = getDatetime.format("YYYY-MM-DD");
    let disDay_Datetime_DDMMYYYY = getDatetime.format("DD-MM-YYYY");
    let disMonth_Datetime = getDatetime.format("MMM");
    let disMonthFull_Datetime = getDatetime.format("MMMM");
    let disMonthYear_Datetime = getDatetime.format("MM/YYYY");
    let disMonthFullYear_Datetime = getDatetime.format("MMMM YYYY");
    let val_Datetime = getDatetime.format("YYYY-MM-DD HH:mm:ss");
    let disFullDatetime = getDatetime.format("DD MMM yyyy HH:mm:ss")
    let Datetime_Datetime = new Date(
      getDatetime.format("MMMM DD, YYYY HH:mm:ss"),
    );

    let return_dis_DateTime =
      dis_Datetime === "Invalid date" ? "-" : dis_Datetime;
    let return_disDay_Datetime =
      disDay_Datetime === "Invalid date" ? "-" : disDay_Datetime;
    let return_disDay_Datetime_YYYYMMDD =
      disDay_Datetime_YYYYMMDD === "Invalid date"
        ? "-"
        : disDay_Datetime_YYYYMMDD;
    let return_disDay_Datetime_DDMMYYYY =
      disDay_Datetime_DDMMYYYY === "Invalid date"
        ? "-"
        : disDay_Datetime_DDMMYYYY;
    let return_disMonth_Datetime =
      disMonth_Datetime === "Invalid date" ? "-" : disMonth_Datetime;
    let return_disMonthFull_Datetime =
      disMonthFull_Datetime === "Invalid date" ? "-" : disMonthFull_Datetime;
    let return_disMonthYear_Datetime =
      disMonthYear_Datetime === "Invalid date" ? "-" : disMonthYear_Datetime;
    let return_disMonthFullYear_Datetime =
      disMonthFullYear_Datetime === "Invalid date"
        ? "-"
        : disMonthFullYear_Datetime;
    let return_val_DateTime =
      val_Datetime === "Invalid date" ? "-" : val_Datetime;
    let return_val_Datetime_Datetime =
      Datetime_Datetime === "Invalid date" ? "-" : Datetime_Datetime;

    let return_disFullDatetime = disFullDatetime === "Invalid date" ? "-" : disFullDatetime
    let returnVal = {
      val: return_val_DateTime,
      val_datetime: return_val_Datetime_Datetime,
      val_datetime_YYYYMMDD: return_disDay_Datetime_YYYYMMDD,
      val_datetime_DDMMYYYY: return_disDay_Datetime_DDMMYYYY,
      dis: return_dis_DateTime,
      disDay: return_disDay_Datetime,
      disMonth: return_disMonth_Datetime,
      disMonthFull: return_disMonthFull_Datetime,
      disMonthYear: return_disMonthYear_Datetime,
      disMonthFullYear: return_disMonthFullYear_Datetime,
      disFullDatetime: return_disFullDatetime,
    };

    return returnVal;
  } catch (err) {
    let returnVal = {
      val: "-",
      val_datetime: new Date(),
      val_datetime_YYYYMMDD: new Date(),
      dis: "-",
      disDay: "-",
      disMonth: "-",
      disMonthFull: "-",
      disMonthYear: "-",
      disMonthFullYear: "-",
    };

    c_LogWebError(pageName, functionName, err);
    return returnVal;
  }
};

// return all data selected 
function c_getTableSelectedData(oriData, dataSelected) {
  let promise = new Promise(function (resolve) {
    if (dataSelected.length === undefined) { //no data or one data selected
      if (dataSelected.key === undefined) { //no data selected
        resolve([]);
      } else {
        var data = oriData; //copy data
        if (oriData.length === undefined) { //only one ori data or no ori data
          if (oriData.key === dataSelected.key) { //selected data same as ori data => remove the data
            resolve([]);
          } else { //selected data not same with ori data => add the data into array
            data.push(dataSelected);
            resolve(data);
          }
        } else {
          if (oriData.filter(d => d.key === dataSelected.key).length > 0) { //ori data contains selected data => remove selected data from ori data
            const newData = data.filter(d => d.key !== dataSelected.key);
            resolve(newData);
          } else {
            data.push(dataSelected); //ori data does not contain selected data =>add the data into array
            resolve(data);
          }
        }
      }
    } else {
      resolve(dataSelected);
    }
  });
  return promise;
};

const c_openURLWithPostRequest = (url, params, linkPage) => {
  var form = document.createElement("form");
  form.setAttribute("method", "post");
  form.setAttribute("action", url);
  form.setAttribute("onsubmit", `window.open('about:blank','${linkPage}');`);
  form.setAttribute("target", linkPage);
  for (var p in params) {
    if (params.hasOwnProperty(p)) {
      if (typeof params[p] === "object") {
        var select = document.createElement('select');
        select.name = p;
        select.id = p;
        select.multiple = true;
        if (params[p].length !== 0) {
          for (var i = 0; i < params[p].length; i++) {
            var opt = document.createElement('option');
            opt.id = params[p][i];
            opt.value = params[p][i];
            opt.selected = true;
            select.appendChild(opt);
          }
        } else {
          var opt = document.createElement('option');
          opt.id = "-1";
          opt.value = "-1";
          opt.selected = true;
          select.appendChild(opt);
        }
        form.appendChild(select);
      } else {
        var input = document.createElement('input');
        input.name = p;
        input.type = 'hidden';
        input.value = params[p];
        form.appendChild(input);
      }
    }
  }
  document.body.appendChild(form);
  form.submit();
  document.body.removeChild(form);
};

export {
  c_TITLE,
  DEFAULT_TIMEOUT,
  ADD_EDIT_TIMEOUT,
  ACTIVATE_DELETE_TIMEOUT,
  GET_ALLDATA_TIMEOUT,
  GET_OPTIONDATA_TIMEOUT,
  EMAIL_REGEX,
  PASSWORD_REGEX,
  NUMBER_ALPHABET_REGEX,
  NUMBER_REGEX,
  PHONE_NUMBER_REGEX,
  FREETEXT_REGEX,
  c_getApiUrl,
  c_getWebUrl,
  c_getWebLoginUrl,
  c_GetFunctionName,
  c_LogWebError,
  c_EncryptData,
  c_Dis_DateTime,
  c_getTableSelectedData,
  c_openURLWithPostRequest,
};