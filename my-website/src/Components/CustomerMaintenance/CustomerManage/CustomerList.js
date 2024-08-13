import React, { useState } from 'react';
import * as Comp from "../../../Common/CommonComponents";
import CssClass from "../../../Styles/common.module.css";
import CustomerUpdate from './CustomerUpdate';
import CustomerDelete from './CustomerDelete';
// import CustomerResetPassword from './CustomerResetPassword';
// import CustomerUnlock from './CustomerUnlock';
import CustomerActivator from './CustomerActivator';
import { MdDeleteForever } from "react-icons/md";
// import { MdLockReset } from "react-icons/md";
import { RxUpdate } from "react-icons/rx";
import { SiReactivex } from "react-icons/si";
// import { FaUnlockAlt } from "react-icons/fa";

const CustomerList = (props) => {
  //#region React Hooks
  const [showUpdate, setShowUpdate] = useState(false);
  const [showDelete, setShowDelete] = useState(false);
  // const [showResetPassword, setShowResetPassword] = useState(false);
  // const [showUnlock, setShowUnlock] = useState(false);
  const [showActivator, setShowActivator] = useState(false);
  const [editData, setEditData] = useState([]);
  //#endregion

  //#region Modal Show/Hide
  const showUpdateModal = (row) => {
    setShowUpdate(!showUpdate);
    setShowDelete(false);
    setEditData(row);
  };

  const hideUpdateModal = () => {
    setShowUpdate(!showUpdate);
    setEditData(null);
  };

  const showDeleteModal = (row) => {
    setShowDelete(!showDelete);
    setEditData(row);
  };

  const hideDeleteModal = () => {
    setShowDelete(!showDelete);
    setEditData(null);
  };

  // const showResetPasswordModal = (row) => {
  //   setShowResetPassword(!showResetPassword);
  //   setEditData(row);
  // };

  // const hideResetPasswordModal = () => {
  //   setShowResetPassword(!showResetPassword);
  //   setEditData(null);
  // };

  // const showUnlockModal = (row) => {
  //   setShowUnlock(!showUnlock);
  //   setEditData(row);
  // };

  // const hideUnlockModal = () => {
  //   setShowUnlock(!showUnlock);
  //   setEditData(null);
  // };

  const showActivatorModal = (e, row) => {
    setShowActivator(!showActivator);
    setEditData(row);
  };

  const hideActivatorModal = () => {
    setShowActivator(!showActivator);
    setEditData(null);
  };
  //#endregion

  //#region Table Columns
  const columns = [
    // {
    //   accessorKey: "CUSTOMER_ID",
    //   header: "Customer ID",
    //   size: 100,
    // },
    {
      accessorKey: "CUSTOMER_NAME",
      header: "Customer Name",
      size: 100,
    },
    {
      accessorKey: "EMAIL",
      header: "Email",
      size: 150,
    },
    {
      accessorKey: "PHONE_NO",
      header: "Phone No",
      size: 100,
    },
    {
      accessorKey: "ADDRESS",
      header: "Address",
      size: 200,
    },
    {
      accessorKey: "COMPANY_NAME",
      header: "Company Name",
      size: 150,
    },
    // {
    //   accessorKey: "ROLE_NAME",
    //   header: "User Role",
    //   size: 80,
    // },
    {
      accessorKey: "LAST_ACCESS_DATETIME",
      header: "Last Access Date Time",
      size: 200,
    },
    {
      id: "STATUS",
      header: "Status",
      size: 80,
      accessorFn: (row) => {
        const className =
          row.STATUS === "Active" ? CssClass.activeStatus : CssClass.inactiveStatus;
        return <span className={className}>{row.STATUS}</span>;
      },
    },
    {
      accessorKey: "UPDATE_ID",
      header: "Last Update By",
      size: 120,
    },
    {
      accessorKey: "UPDATE_DATETIME",
      header: "Last Update Date Time",
      size: 200,
    },
    {
      id: "Action",
      header: "Action",
      size: 300,
      classes: CssClass.tdBtnActionWrapperOne,
      accessorFn: (row) => (
        <div>
          <span>
            <Comp.Button
              id="btnEdit"
              type="general"
              onClick={() => showUpdateModal(row)}
            >
              UPDATE
              <RxUpdate className='icon' style={{ marginBottom: '5px', fontSize: '17px', marginLeft: '5px' }} />
            </Comp.Button>
            {/* <Comp.Button
              id="btnEdit"
              type="general"
              style={{width: 'auto'}}
              onClick={() => showResetPasswordModal(row)}
            >
              RESET PASS
              <MdLockReset className='icon' style={{marginBottom: '5px', fontSize: '17px', marginLeft: '5px'}}/>
            </Comp.Button> */}
            {/* <Comp.Button
              id="btnUnlock"
              type="general"
              style={{width: 'auto'}}
              onClick={() => showUnlockModal(row)}
            >
              UNLOCK
              <FaUnlockAlt className='icon' style={{marginBottom: '5px', fontSize: '15px', marginLeft: '5px'}}/>
            </Comp.Button> */}
            <Comp.Button
              id="btnActivator"
              type="activator"
              onClick={(e) => showActivatorModal(e, row)}
            >
              {row.STATUS === "Active" ? "DEACTIVE" : "ACTIVE"}
              <SiReactivex className='icon' style={{ marginBottom: '5px', fontSize: '15px', marginLeft: '5px' }} />
            </Comp.Button>
            <Comp.Button
              id="btnDelete"
              type="delete"
              onClick={() => showDeleteModal(row)}
            >
              DELETE
              <MdDeleteForever className='icon' style={{ marginBottom: '5px', fontSize: '17px', marginLeft: '5px' }} />
            </Comp.Button>
          </span>
        </div>
      ),
    },
  ];
  //#endregion

  return (
    <>
      <Comp.Table
        columns={columns}
        data={props.data}
        invisibleCol={{ Action: (props.actionAuthority.WRITE !== "Y" ? false : true) }}
      />
      <CustomerUpdate
        page={props.page}
        module={props.module}
        showHide={showUpdate}
        editData={editData}
        onHide={hideUpdateModal}
        onReload={props.onReload}
        onLoading={props.onLoading}
      />
      {/* <CustomerResetPassword
        page={props.page}
        module={props.module}
        showHide={showResetPassword}
        editData={editData}
        onHide={hideResetPasswordModal}
        onReload={props.onReload}
        onLoading={props.onLoading}
      /> */}
      {/* <CustomerUnlock
        page={props.page}
        module={props.module}
        showHide={showUnlock}
        editData={editData}
        onHide={hideUnlockModal}
        onReload={props.onReload}
        onLoading={props.onLoading}
      /> */}
      <CustomerActivator
        page={props.page}
        module={props.module}
        showHide={showActivator}
        editData={editData}
        onHide={hideActivatorModal}
        onReload={props.onReload}
        onLoading={props.onLoading}
      />
      {showDelete && (
        <CustomerDelete
          page={props.page}
          module={props.module}
          showHide={showDelete}
          editData={editData}
          onHide={hideDeleteModal}
          onReload={props.onReload}
          onLoading={props.onLoading}
        />
      )}
    </>
  );
};

export default CustomerList;
