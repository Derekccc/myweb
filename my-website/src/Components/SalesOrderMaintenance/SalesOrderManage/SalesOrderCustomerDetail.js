import React, { useState, useEffect } from 'react';
import { Modal } from 'react-bootstrap';
import { GrUpdate } from 'react-icons/gr';
import { BiSolidUserPin, BiSolidRename } from 'react-icons/bi';
import { AiTwotoneMail } from 'react-icons/ai';
import { FaUserAstronaut } from 'react-icons/fa';
import http from '../../../Common/http-common'; // Custom HTTP utility
import { toast } from 'react-toastify';
import * as Comp from '../../../Common/CommonComponents'; // Custom components
import * as common from '../../../Common/common'; // Custom utility functions

const SalesOrderCustomerDetail = (props) => {
  const [customerDetails, setCustomerDetails] = useState([]);





  const formatDateTime = (dateTime) => {
    if (!dateTime) return '';
    const date = new Date(dateTime);
    return `${date.toLocaleDateString()} ${date.toLocaleTimeString()}`;
  };

  return (
    <Modal show={props.showHide} onHide={hideModal} centered>
      <Modal.Header closeButton>
        <div style={{ backgroundColor: '#FFDEAD', width: '100%' }}>
          <Modal.Title>
            CUSTOMER DETAILS <GrUpdate className='icon' style={{ fontSize: '20px', color: 'black', marginBottom: '5px' }} />
          </Modal.Title>
        </div>
      </Modal.Header>
      <Modal.Body>
        <div>
          <BiSolidUserPin className='icon' style={{ marginBottom: '5px', fontSize: '20px', marginRight: '10px' }} />
          <label htmlFor='ORDER_DATETIME'>Order Date:</label>
          <Comp.Input
            id='ORDER_DATETIME'
            type='text'
            className='form-control'
            value={formatDateTime(customerDetails.ORDER_DATETIME)}
            readOnly
          />
        </div>
        <div>
          <BiSolidRename className='icon' style={{ marginBottom: '5px', fontSize: '20px', marginRight: '10px' }} />
          <label htmlFor='CUSTOMER_NAME'>Customer Name:</label>
          <Comp.Input
            id='CUSTOMER_NAME'
            name='Customer Name'
            type='text'
            className='form-control'
            value={customerDetails.CUSTOMER_NAME}
            readOnly
          />
        </div>
        <div>
          <AiTwotoneMail className='icon' style={{ marginBottom: '5px', fontSize: '20px', marginRight: '10px' }} />
          <label htmlFor='EMAIL'>Email:</label>
          <Comp.Input
            id='EMAIL'
            name='Email'
            type='text'
            className='form-control'
            value={customerDetails.EMAIL}
            readOnly
          />
        </div>
        <div>
          <FaUserAstronaut className='icon' style={{ marginBottom: '5px', fontSize: '20px', marginRight: '10px' }} />
          <label htmlFor='ADDRESS'>Address:</label>
          <Comp.Input
            id='ADDRESS'
            name='Address'
            className='form-control'
            value={customerDetails.ADDRESS}
            readOnly
          />
        </div>
        <div>
          <FaUserAstronaut className='icon' style={{ marginBottom: '5px', fontSize: '20px', marginRight: '10px' }} />
          <label htmlFor='COMPANY_NAME'>Company Name:</label>
          <Comp.Input
            id='COMPANY_NAME'
            name='Company Name'
            className='form-control'
            value={customerDetails.COMPANY_NAME}
            readOnly
          />
        </div>
        <div>
          <FaUserAstronaut className='icon' style={{ marginBottom: '5px', fontSize: '20px', marginRight: '10px' }} />
          <label htmlFor='PHONE_NO'>Phone Number:</label>
          <Comp.Input
            id='PHONE_NO'
            name='Phone No'
            className='form-control'
            value={customerDetails.PHONE_NO}
            readOnly
          />
        </div>
      </Modal.Body>
      <Modal.Footer>
        <div style={{ textAlign: 'center', width: '100%' }}>
          <Comp.Button id='btnCancel' type='cancel' onClick={hideModal}>
            Cancel
          </Comp.Button>
        </div>
      </Modal.Footer>
    </Modal>
  );
};

export default SalesOrderCustomerDetail;
