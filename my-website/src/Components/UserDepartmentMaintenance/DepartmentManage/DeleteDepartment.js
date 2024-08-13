import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.min.css';
import http from '../../../Common/http-common';
import { useCookies } from 'react-cookie';
import { Modal, Button } from 'react-bootstrap';
import { MdOutlineDeleteSweep } from "react-icons/md";

const DeleteDepartment = (props) => {
  const [cookies] = useCookies([]);
  const userId = cookies.USER_ID;

  const [confirmDelete, setConfirmDelete] = useState(false);

  useEffect(() => {
    if (props.editData) {
      setConfirmDelete(true);
    }
  }, [props.editData]);

  const deleteDepartment = () => {
    const departmentToDelete = {
      DEPARTMENT_ID: props.editData.DEPARTMENT_ID,
      UPDATE_ID: userId,
      FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
    };

    props.onLoading(true, "Deleting department...");

    http.put('api/department/DeleteDepartment', departmentToDelete, { timeout: 10000 })
      .then(() => {
        toast.success("Department successfully deleted.");
        props.onReload();
        props.onHide();
      })
      .catch((err) => {
        toast.error("Failed to delete department. Please try again.");
        console.error(err);
      })
      .finally(() => {
        props.onLoading(false);
      });
  };

  const handleCancel = () => {
    setConfirmDelete(false);
    props.onHide();
  };

  return (
    <>
      {confirmDelete && (
        <Modal show={confirmDelete} onHide={handleCancel}>
          <Modal.Header closeButton>
          <div style={{ backgroundColor: "#FFDEAD", width: "100%" }}>
            <Modal.Title>Confirm Delete &nbsp; <MdOutlineDeleteSweep className='icon' style={{ fontSize: '30px', color: 'black', marginBottom: '5px'}}/></Modal.Title>
          </div>
          </Modal.Header>
          <Modal.Body>
            <p style={{color: 'red'}}><b>Are you sure you want to "DELETE" the department ?</b></p>
            <hr/>
            <p><b>DEPARTMENT NAME :</b></p>
            <p><b>{"---> ( " + props.editData.DEPARTMENT_NAME + " ) "}</b></p>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={handleCancel}>
              Cancel
            </Button>
            <Button variant="primary" onClick={deleteDepartment}>
              Confirm
            </Button>
          </Modal.Footer>
        </Modal>
      )}
    </>
  );
};

export default DeleteDepartment;