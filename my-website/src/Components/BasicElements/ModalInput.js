import React, { useRef, useImperativeHandle } from "react";

import classes from "../../Styles/common.module.css";

/**
* Input Component
* 1. id : string --component id                  ! required ! not duplicable
* 2. type : string --input type (default text)
* 3. label : string --text on the input column
* 4. value : string --value of the input
* 5. onChange : function --when component onChange
* 6. onBlur : function --when component onBlur
* 7. name : string --component name (required for error display)
* 8. className : string --css class (default "form-control")
* 9. readOnly : boolean --display text only (default false)
* 10. disabled : boolean --could not be input (default false) 
* 11. checked : boolean --specific for checkbox
* 12. accept : string --specific for files
* @public
*/
const Input = React.forwardRef((props, ref) => {
  const inputRef = useRef();
  const label = props.label === undefined ? "" : props.label;
  const errorMessage = props.errorMessage === undefined ? "" : props.errorMessage;
  const isSearch = props.isSearch === undefined ? false : true;

  const activate = () => {
    inputRef.current.focus();

  };

  useImperativeHandle(ref, () => {
    return {
      focus: activate,
    };
  });

  const onChangeFn = (event) => {
    if (props.type === "file" && event.target.files) {
      const fileSizeLimit = props.sizeLimit ?? "15000"; // kb
      const acceptFileType = props.accept ? props.accept.split(", ") : ["*"];
      var extString = filetypeToExtString(acceptFileType);
      var extTestReg = new RegExp(extString, "i");
      if (event.target.value !== null && event.target.value !== "") {
        const files = event.target.files;
        const fileNum = files.length;
        for (var i = 0; i < fileNum; i++) {
          if((!checkFileExtension(extTestReg, files[i].name)) || (files[i].size > (fileSizeLimit * 1000))) { // .size return in bytes
            event.target.value = null; // set value will trigger onChange again, enhancement shd beware of this
            break;
          }
        }
      }
    }
    props.onChange(event);
  }

  function filetypeToExtString(filetype) {
    var currExtension = filetype.pop();
    var allowedExtensions = "(\\" + currExtension;
    currExtension = filetype.pop();
    while (currExtension !== undefined) {
      allowedExtensions += "|\\" + currExtension;
      currExtension = filetype.pop();
    }
    allowedExtensions += ")$";
    return allowedExtensions;
  }

  function checkFileExtension(extensionTestReg, filename){
    if (filename === "" || !extensionTestReg.exec(filename)) {
      return false;
    } else return true;
  }

  return (
    <div
      hidden={props.hidden}
      className={`form-group ${label !== undefined && !isSearch && classes.modalGroup
        }`}
    >
      {props.label !== undefined && (
        <label className={classes.colFormLabel} htmlFor={props.id}>
          {props.label}
        </label>
      )}
      <div className={classes.modalPopupItem}>
        <input
          className={`${props.className !== undefined ? props.className : "form-control"
            } ${props.label !== undefined &&
            props.isValid === false &&
            classes.invalidField
            }`}
          ref={inputRef}
          type={props.type !== undefined ? props.type : "text"}/*'file' for file upload */
          id={props.id}
          value={props.type === "file" ? undefined : props.value !== undefined ? props.value : ""}
          name={props.name !== undefined ? props.name : ""}
          onChange={(event) => onChangeFn(event)}
          onBlur={props.onBlur}
          readOnly={props.readOnly}
          disabled={props.disabled}
          checked={props.checked}
          accept={props.accept}/**For file upload **/
          placeholder={props.placeholder}/**For text input only **/
        />
        {errorMessage !== "" && (
          <div className={classes.invalidMessage}>{errorMessage}</div>
        )}
      </div>
    </div>
  );
});

export default Input;
