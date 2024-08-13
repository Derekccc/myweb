import React, { useRef, useImperativeHandle } from "react";

import classes from "../../Styles/common.module.css";

const TextArea = React.forwardRef((props, ref) => {
  const inputRef = useRef();

  const activate = () => {
    inputRef.current.focus();
  };

  useImperativeHandle(ref, () => {
    return {
      focus: activate,
    };
  });

  return (
    <div
       className={`form-group ${
         props.label !== undefined && classes.modalGroup
       }`}
    >
      {props.label !== undefined && (
        <label className={classes.colFormLabel} htmlFor={props.id}>
          {props.label}
        </label>
      )}
      <div className={classes.modalPopupItem}>
        <textarea
          className={`${
            props.className !== undefined ? props.className : "form-control"
          } ${
            props.label !== undefined &&
            props.isValid === false &&
            classes.invalidField
          }`}
          ref={inputRef}
          id={props.id}
          value={props.value !== undefined ? props.value : ""}
          name={props.name !== undefined ? props.name : ""}
          onChange={props.onChange}
          onBlur={props.onBlur}
          readOnly={props.readOnly}
          disabled={props.disabled}
          checked={props.checked}
          rows={props.rows}
        />
        { props.isValid === false && (
          <div className={classes.invalidMessage}>{props.errorMessage}</div>
        ) }
      </div>
    </div>
  );
});

export default TextArea;
