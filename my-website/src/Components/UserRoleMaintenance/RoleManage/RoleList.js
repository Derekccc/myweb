import { useState } from 'react';
import CssClass from "../../../Styles/common.module.css";
import * as Comp from "../../../Common/CommonComponents";
import * as common from "../../../Common/common";
import UpdateRole from './UpdateRole';
import RoleActivator from './RoleActivator';
import DeleteRole from './DeleteRole';
import { MdDeleteForever } from "react-icons/md";
import { RxUpdate } from "react-icons/rx";
import { SiReactivex } from "react-icons/si";

const RoleList = (props) => {
  //#region React Hooks
  const [showUpdate, setShowUpdate] = useState(false);
  const [showActivator, setShowActivator] = useState(false);
  const [showDelete, setShowDelete] = useState(false);
  const [editData, setEditData] = useState({});
  //#endregion

  //#region Modal Show/Hide
  const showUpdateModal = (row) => {
    setShowUpdate(true);
    setShowDelete(false);
    setEditData(row);
  };

  const hideUpdateModal = () => {
    setShowUpdate(false);
    setEditData(null);
  };

  const showActivatorModal = (row) => {
    setShowActivator(!showActivator)
    setEditData(row);
  };

  const hideActivatorModal = () => {
    setShowActivator(!showActivator);
    setEditData(null);
  };

  const showDeleteModal = (row) => {
    setShowUpdate(false);
    setShowDelete(true);
    setEditData(row);
  };

  const hideDeleteModal = () => {
    setShowDelete(false);
    setEditData(null);
  };
  
  //#endregion

  //#region Table Columns
  const columns = [
    {
      accessorKey: "ROLE_NAME",
      header: "Role Name",
      size: 120,
    },
    {
      accessorKey: "ROLE_DESC",
      header: "Description",
      size: 180,
      Cell: ({ cell }) => (cell.getValue() === null || cell.getValue() === "") ? (<><label> - </label></>) : cell.getValue(),
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
      Cell: ({ cell }) => (cell.getValue() === null || cell.getValue() === "") ? (<><label> - </label></>) : cell.getValue(),
    },
    {
      accessorKey: "UPDATE_DATETIME",
      header: "Last Update Date Time",
      size: 200,
      muiTableHeadCellFilterTextFieldProps: {
        type: 'date',
      },
      sortingFn: 'datetime',
      Cell: ({ cell }) => common.c_Dis_DateTime(cell.getValue()).dis,
    },
    {
      id: "Action",
      header: "Action",
      size: 400,
      classes: CssClass.tdBtnActionWrapperTwo,
      accessorFn: (row) => (
        <div style={{ textAlign: "-webkit-center" }}>
          <span>
            <Comp.Button
              id="btnEdit"
              type="general"
              onClick={() => showUpdateModal(row)}
            >
              UPDATE
              <RxUpdate className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button>
            <Comp.Button
              id="btnActivator"
              type="activator"
              onClick={() => showActivatorModal(row)}
            >
              {row.STATUS === "Active" ? "DEACTIVE" : "ACTIVATE"}
              <SiReactivex className='icon' style={{marginBottom: '5px', fontSize: '15px', marginLeft: '5px'}}/>
            </Comp.Button>
            <Comp.Button
              id="btnDelete"
              type="delete"
              onClick={() => showDeleteModal(row)}
              
            >
              DELETE
              <MdDeleteForever className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
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
        invisibleCol={{ Action: (props.actionAuthority.WRITE !== "Y") ? false : true }}
      />
      <>
       
          <UpdateRole
            page={props.page}
            module={props.module}
            editData={editData}
            showHide={showUpdate}
            onHide={hideUpdateModal}
            onReload={props.onReload}
            onLoading={props.onLoading}
          />

          <RoleActivator
            page={props.page}
            module={props.module}
            showHide={showActivator}
            editData={editData}
            onHide={hideActivatorModal}
            onReload={props.onReload}
            onLoading={props.onLoading}
          />
        
        {showDelete && (
          <DeleteRole
            page={props.page}
            module={props.module}
            editData={editData}
            showHide={showDelete}
            onHide={hideDeleteModal}
            onReload={props.onReload}
            onLoading={props.onLoading}
          />
          
        )}
      </>
    </>
  );
};

export default RoleList;
