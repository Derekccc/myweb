import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.min.css';
import http from '../../../Common/http-common';
import { useCookies } from 'react-cookie';
import { Modal, Button } from 'react-bootstrap';
import { MdOutlineDeleteSweep } from "react-icons/md";

const ProductDelete = (props) => {
  const [cookies] = useCookies([]);
  const userId = cookies.USER_ID;

  const [confirmDelete, setConfirmDelete] = useState(false);

  useEffect(() => {
    if (props.editData) {
      setConfirmDelete(true);
    }
  }, [props.editData]);

  const deleteProduct = () => {
    const productToDelete = {
      PRODUCT_ID: props.editData.PRODUCT_ID,
      UPDATE_ID: userId,
      FROM_SOURCE: { SOURCE: "WEB", MODULE_ID: props.module },
    };

    props.onLoading(true, "Deleting product...");

    http.put('api/product/DeleteProduct', productToDelete, { timeout: 10000 })
      .then(() => {
        toast.success("Product successfully deleted.");
        props.onReload();
        props.onHide();
      })
      .catch((err) => {
        toast.error("Failed to delete product. Please try again.");
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
            <p style={{color: 'red'}}><b>Are you sure you want to "DELETE" the product ?</b></p>
            <hr/>
            <p><b>Product Name : {props.editData.PRODUCT_NAME}</b></p>
            {/* <p><b>{"---> ( " + props.editData.PRODUCT_NAME + " ) "}</b></p> */}
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={handleCancel}>
              Cancel
            </Button>
            <Button variant="primary" onClick={deleteProduct}>
              Confirm
            </Button>
          </Modal.Footer>
        </Modal>
      )}
    </>
  );
};

export default ProductDelete;