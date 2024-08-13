import { useState } from 'react';
import CssClass from "../../../Styles/common.module.css";
import * as Comp from "../../../Common/CommonComponents";
import * as common from "../../../Common/common";
import SalesOrderUpdate from './SalesOrderUpdate';
import SalesOrderReviewer from './SalesOrderReviewer';
// import SalesOrderCustomerDetail from './SalesOrderCustomerDetail';
// import ProductDelete from './ProductDelete';
import { MdDeleteForever } from "react-icons/md";
import { RxUpdate } from "react-icons/rx";
import { SiReactivex } from "react-icons/si";


const SalesOrderList = (props) => {
  //#region React Hooks
  const [showUpdate, setShowUpdate] = useState(false);
  const [showReview, setShowReview] = useState(false);
  // const [showCustomerDetail, setShowCustomerDetail] = useState(false);
//   const [showDelete, setShowDelete] = useState(false);
  const [editData, setEditData] = useState([]);
  //#endregion

//   #region Modal Show/Hide
  const showUpdateModal = (row) => {
    setShowUpdate(true);
    setEditData(row);
  };

  const hideUpdateModal = () => {
    setShowUpdate(false);
    setEditData(null);
  };

  const showReviewModal = (row) => {
    setShowReview(true);
    setEditData(row);
  };

  const hideReviewModal = () => {
    setShowReview(false);
    setEditData(null);
  };

  // const showCustomerDetailModal = (row) => {
  //   setShowCustomerDetail(true);
  //   setEditData(row);
  // };

  // const hideCustomerDetailModal = () => {
  //   setShowCustomerDetail(false);
  //   setEditData(null);
  // };

//   const showDeleteModal = (row) => {
//     setShowDelete(true);
//     setShowUpdate(false);
//     setEditData(row);
//   };

//   const hideDeleteModal = () => {
//     setShowDelete(false);
//     setEditData(null);
//   };
  //#endregion
  
  // function c_Dis_Date(ORDER_DATETIME) {
  //   const dateObj = new Date(ORDER_DATETIME); 
  //   const formattedDate = dateObj.toISOString().split('T')[0]; // Get date part only in 'YYYY-MM-DD' format
  //   return { dis: formattedDate };
  // }

  function c_Dis_Date(ORDER_DATETIME) {
    const formattedDate = ORDER_DATETIME.split('T')[0]; // Get date part only in 'YYYY-MM-DD' format
    return { dis: formattedDate };
  }
  

  //#region Table Columns
  const columns = [
    {
      accessorKey: "SALES_ORDER_ID",
      header: "Sales Order ID",
      size: 120,
    },
    {
      accessorKey: "CUSTOMER_NAME",
      header: "Customer Name",
      size: 180,
      Cell: ({ cell }) => (cell.getValue() === null || cell.getValue() === "") ? (<><label> - </label></>) : cell.getValue(),
    },
    {
        accessorKey: "TOTAL_AMOUNT",
        header: "Total Sales Amount / RM",
        size: 100,
        Cell: ({ cell }) => (cell.getValue() === null || cell.getValue() === "") ? (<><label> - </label></>) : cell.getValue(),
    },
    {
      accessorKey: "ORDER_DATETIME",
      header: "Order Date",
      size: 120,
      muiTableHeadCellFilterTextFieldProps: {
          type: 'date',
        },
        sortingFn: "datetime",
        Cell: ({ cell }) => c_Dis_Date(cell.getValue()).dis,
    },
    {
      id: "REVIEW",
      header: "Review",
      size: 80,
      accessorFn: (row) => {
        const className = 
          row.REVIEW === "Accept" ? CssClass.acceptReview : CssClass.rejectReview;
        return <span className={className}>{row.REVIEW}</span>;
      },
    },
    {
        accessorKey: "ORDER_STATUS",
        header: "Order Status",
        size: 80,
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
      sortingFn: "datetime",
      Cell: ({ cell }) => common.c_Dis_DateTime(cell.getValue()).dis,
    },
    {
      id: "Action",
      header: "Action",
      size: 250,
      classes: CssClass.tdBtnActionWrapperTwo,
      accessorFn: (row) => (
        <div style={{textAlign: "-webkit-center"}}>
          <span>
            <Comp.Button
              id= "btnEdit"
              type= "general"
              onClick= {() => showUpdateModal(row)}
            >
              UPDATE
              <RxUpdate className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button>
            <Comp.Button
              id= "btnReview"
              type= "delete"
              onClick= {() => showReviewModal(row)}
            >
              {row.REVIEW === "Accept" ? "REJECT" : "ACCEPT"}
              <SiReactivex className='icon' style={{ marginBottom: '5px', fontSize: '15px', marginLeft: '5px' }} />
            </Comp.Button>
            {/* <Comp.Button
              id= "btnView"
              type= "general"
              onClick= {() => showCustomerDetailModal(row)}
            >
              VIEW
              <RxUpdate className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button> */}
            {/* <Comp.Button
              id="btnDelete"
              type="delete"
              onClick={() => showDeleteModal(row)}
            >
              DELETE
              <MdDeleteForever className='icon' style={{marginBottom: '5px', fontSize: '20px', marginLeft: '5px'}}/>
            </Comp.Button> */}
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
      invisibleCol= {{ Action: (props.actionAuthority.WRITE !== "Y") ? false : true }}
    />
    <>
      <SalesOrderUpdate
        page={props.page}
        module={props.module}
        editData={editData}
        showHide={showUpdate}
        onHide={hideUpdateModal}
        onReload={props.onReload}
        onLoading={props.onLoading}
      />
      <SalesOrderReviewer
        page={props.page}
        module={props.module}
        editData={editData}
        showHide={showReview}
        onHide={hideReviewModal}
        onReload={props.onReload}
        onLoading={props.onLoading}
      />

      {/* <SalesOrderCustomerDetail
        page={props.page}
        module={props.module}
        editData={editData}
        showHide={showCustomerDetail}
        onHide={hideCustomerDetailModal}
        onReload={props.onReload}
        onLoading={props.onLoading}
      /> */}
    </>
    </>
  );
};

export default SalesOrderList;