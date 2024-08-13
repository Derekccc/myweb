import classes from "../../Styles/common.module.css";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";

const DateTimePicker = (props) => {
  const errorMessage = props.errorMessage === undefined ? "" : props.errorMessage;
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
        <DatePicker
          {...props}
          className={`${
            props.className !== undefined ? props.className : "form-control"
          } ${
            props.label !== undefined &&
            props.isValid === false &&
            classes.invalidField
          }`}
          dateFormat={
            props.dateFormat !== undefined ? props.dateFormat : "dd/MM/yyyy"
          }
          isClearable={props.disabled !== undefined ? !props.disabled : true}
          disabled={ props.disabled !== undefined ? props.disabled : false}
        />
        {errorMessage !== "" && (
          <div className={classes.invalidMessage}>{errorMessage}</div>
        )}
      </div>
    </div>
  );
};

export default DateTimePicker;
