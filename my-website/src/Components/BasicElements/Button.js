import React from "react";
import classes from "../../Styles/button.module.css";

const GeneralButton = (props) => {
  let buttonClass = "";
  let sizeClass = "";

  if (props.type === "general") {
    buttonClass = classes.btnAll;
  } else if (props.type === "excel") {
    buttonClass = classes.btnExcel;
  } else if (props.type === "cancel") {
    buttonClass = classes.btnCancel;
  } else if (props.type === "delete") {
    buttonClass = classes.btnDelete;
  } else if (props.type === "activator") {
    buttonClass = classes.btnActivator;
  } else {
    buttonClass = classes.btnAll;
  }

  if (props.size === "sm") {
    sizeClass = classes.smallBtnSize;
  } else if (props.size === "xl") {
    sizeClass = classes.xlBtnSize;
  } else if (props.size === "xxl") {
    sizeClass = classes.xxlBtnSize;
  } else {
    sizeClass = classes.normalBtnSize;
  }

  return (
    <>
      <button
        style={props.style === undefined ? {} : props.style}
        className={`${classes.btn} ${buttonClass} ${sizeClass}`}
        type={props.type !== "submit" ? "button" : "submit"}
        id={props.id}
        onClick={props.onClick}
        hidden={props.hidden}
      >
        {props.children}
      </button>
    </>
  );
};

export default GeneralButton;
