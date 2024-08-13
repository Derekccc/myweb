import { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.min.css';
import http from '../../../Common/http-common';
import { useCookies } from 'react-cookie';
import * as Comp from '../../../Common/CommonComponents'
import { Modal } from 'react-bootstrap';
import { MdOutlineDeleteSweep } from "react-icons/md";

const DeleteRole = (props) => {
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);

  const [confirmDelete, setConfirmDelete] = useState(false);

  useEffect(() => {
    if (props.editData) {
      setConfirmDelete(true);
    }
  }, [props.editData]);

  const deleteRole = () => {
    const roleToDelete = {
      ROLE_ID: props.editData.ROLE_ID,
      UPDATE_ID: userId,
      FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
    };

    props.onLoading(true, "Deleting role, Please wait....");

    http.put('api/role/DeleteRole', roleToDelete, { timeout: 10000 })
      .then(() => {
        toast.success("Role successfully deleted.");
        props.onReload();
        props.onHide();
      })
      .catch((err) => {
        toast.error("Failed to delete role. Please try again.");
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
          
            <p style={{color: 'red'}}><b>Are you sure you want to "DELETE" the role ?</b></p>
            <hr/>
            <p><b>ROLE NAME :</b></p>
            <p><b>{"---> ( " + props.editData.ROLE_NAME + " )"} </b></p> 
          </Modal.Body>
          <Modal.Footer>
            <Comp.Button 
              variant="secondary" 
              type='cancel'
              onClick={handleCancel}
            >
              Cancel
            </Comp.Button>
            <Comp.Button 
              variant="primary" 
              type='confirm'
              onClick={deleteRole}
            >
              Confirm
            </Comp.Button>
          </Modal.Footer>
        </Modal>
      )}
    </>
  );
};

export default DeleteRole;
