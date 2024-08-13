import React, { useRef, useImperativeHandle } from "react";

import classes from "../../Styles/common.module.css";

const Input = React.forwardRef((props, ref) => {
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
    <div className={`form-group ${classes.modalGroup}`}>
      <label className={classes.colFormLabel} htmlFor={props.id}>
        {props.label}
      </label>
      <div className={classes.modalPopupItem}>
        <input
          className={props.className}
          ref={inputRef}
          type={props.type}
          id={props.id}
          value={props.value}
          onChange={props.onChange}
          onBlur={props.onBlur}
        />
        {props.isValid === false && (
          <div className={classes.invalidMessage}>{props.errorMessage}</div>
        )}
      </div>
    </div>
  );
});

export default Input;
