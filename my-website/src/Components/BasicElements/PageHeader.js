import React from "react";
import classes from "../../Styles/common.module.css";

const PageHeader = (props) => {
  return (
    <>
      <h1 className={classes.title}>{props.children}</h1>

      {/* <div>
        <nav aria-label={classes.breadcrumb}>
          <ol className={`${classes.breadcrumb} ${classes["arr-bread"]}`}>
            <li>
              <a href="/#">{props.mainMenu}</a>
            </li>

            <li className="active">
              <span>{props.children}</span>
            </li>
          </ol>
        </nav>
      </div> */}
    </>
  );
};

export default PageHeader;
