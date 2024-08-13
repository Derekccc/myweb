import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import { useCookies } from "react-cookie";
import './App.css';



import Home from './Components/Home';

// Login Entrance
import LoginMain from './Components/LoginMain';

// Navigation Bar
import Navbars from './Components/Menu/Navbars';

// For Authority
// import AuthorityHomePage from './Components/UserAuthoritySetup/AuthorityHomePage';
import RAUserInfoTable from './Components/UserTabletSetup/RoleAuthorityMaintenance/RAUserInfoTable';
import AuthorityRolesSetting from './Components/UserAuthoritySetup/AuthorityManage/AuthorityRolesSetting';

// For Role
import RoleHomePage from './Components/UserRoleMaintenance/RoleHomePage';
import RegisterRole from './Components/UserRoleMaintenance/RoleManage/RegisterRole';
import RoleList from './Components/UserRoleMaintenance/RoleManage/RoleList';
import UpdateRole from './Components/UserRoleMaintenance/RoleManage/UpdateRole';
import DeleteRole from './Components/UserRoleMaintenance/RoleManage/DeleteRole';

// For Department
import DepartmentHomePage from './Components/UserDepartmentMaintenance/DepartmentHomePage';
import RegisterDepartment from './Components/UserDepartmentMaintenance/DepartmentManage/RegisterDepartment';
import DepartmentList from './Components/UserDepartmentMaintenance/DepartmentManage/DepartmentList';
import UpdateDepartment from './Components/UserDepartmentMaintenance/DepartmentManage/UpdateDepartment';
import DeleteDepartment from './Components/UserDepartmentMaintenance/DepartmentManage/DeleteDepartment';

// For User
import UserHomePage from './Components/UserMaintenance/UserHomePage';
import RegisterUser from './Components/UserMaintenance/UserManage/RegisterUser';
import UserList from './Components/UserMaintenance/UserManage/UserList';
import UpdateUser from './Components/UserMaintenance/UserManage/UpdateUser';
import DeleteUser from './Components/UserMaintenance/UserManage/DeleteUser';
import UserResetPassword from './Components/UserMaintenance/UserManage/UserResetPassword';

// For Customer
import CustomerHomePage from './Components/CustomerMaintenance/CustomerHomePage';
import CustomerRegister from './Components/CustomerMaintenance/CustomerManage/CustomerRegister';
import CustomerList from './Components/CustomerMaintenance/CustomerManage/CustomerList';
import CustomerUpdate from './Components/CustomerMaintenance/CustomerManage/CustomerUpdate';
import CustomerDelete from './Components/CustomerMaintenance/CustomerManage/CustomerDelete';

// For Product
import ProductHomePage from './Components/ProductMaintenance/ProductHomePage';
import ProductRegister from './Components/ProductMaintenance/ProductManage/ProductRegister';
import ProductList from './Components/ProductMaintenance/ProductManage/ProductList';
import ProductUpdate from './Components/ProductMaintenance/ProductManage/ProductUpdate';
import ProductDelete from './Components/ProductMaintenance/ProductManage/ProductDelete';

// For Sales Order
import SalesOrderHomePage from './Components/SalesOrderMaintenance/SalesOrderHomePage';
import SalesOrderList from './Components/SalesOrderMaintenance/SalesOrderManage/SalesOrderList';

// For Sales Chart
import SalesChartHomePage from './Components/SalesChartMaintenance/SalesChartHomePage';
// import SalesOrderList from './Components/SalesOrderMaintenance/SalesOrderManage/SalesOrderList';

function App() {
  const [cookies, setCookie, removeCookie] = useCookies(['USER_ID']);
  const [isLoggedIn, setIsLoggedIn] = useState(!!cookies.USER_ID);
  const [user, setUser] = useState(null);

  const handleLogin = (userData) => {
    setIsLoggedIn(true);
    setUser(userData);
    setCookie('USER_ID', userData.USER_ID, { path: '/' });
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setUser(null);
    removeCookie('USER_ID', { path: '/' });
  };

  useEffect(() => {
    if (!cookies.USER_ID && window.location.pathname !== "/loginmain") {
      window.location.pathname = "/loginmain";
    }
  }, [cookies.USER_ID]);

  return (
    <Router>
      <div className="App">
        {!isLoggedIn ? (
          <Routes>
            {/* <Route path="/loginmodels/login" element={<Login handleLogin={handleLogin} />} /> */}
            <Route path="/loginmain" element={<LoginMain handleLogin={handleLogin} />} />
            
            <Route path="*" element={<Navigate to="/loginmain" />} />
          </Routes>
        ) : (
          <>
            <Navbars isLoggedIn={isLoggedIn} handleLogout={handleLogout} />
            <Routes>
              <Route path="/" element={<Home />} />

              {/* For Role Route*/}
              <Route path="/rolehomepage" element={<RoleHomePage />} />
              <Route path="/registerrole" element={<RegisterRole />} />
              <Route path="/rolelist" element={<RoleList />} />
              <Route path="/updaterole" element={<UpdateRole />} />
              <Route path="/deleterole" element={<DeleteRole />} />

              {/* For Department Route */}
              <Route path="/departmenthomepage" element={<DepartmentHomePage />} />
              <Route path="/registerdepartment" element={<RegisterDepartment />} />
              <Route path="/departmentlist" element={<DepartmentList />} />
              <Route path="/updatedepartment" element={<UpdateDepartment />} />
              <Route path="/deletedepartment" element={<DeleteDepartment />} />

              {/* For User Route */}
              <Route path='/userhomepage' element={<UserHomePage/>} />
              <Route path="/registeruser" element={<RegisterUser />} />
              <Route path="/userlist" element={<UserList />} />
              <Route path="/updateuser" element={<UpdateUser />} />
              <Route path="/deleteuser" element={<DeleteUser />} />
              <Route path="/userresetpassword" element={<UserResetPassword />} />

              {/* For Authority Route */}
              <Route path="/authorityrolessetting" element={<AuthorityRolesSetting />} />
              <Route path='/rauserinfotable' element={<RAUserInfoTable />} />

              {/* For Customer Route */}
              <Route path="/customerhomepage" element={<CustomerHomePage />} />
              <Route path="/customerregister" element={<CustomerRegister />} />
              <Route path="/customerlist" element={<CustomerList />} />
              <Route path="/customerupdate" element={<CustomerUpdate />} />
              <Route path="/customerdelete" element={<CustomerDelete />} />

              {/* For Product Route */}
              <Route path="/producthomepage" element={<ProductHomePage />} />
              <Route path="/productregister" element={<ProductRegister />} />
              <Route path="/productlist" element={<ProductList />} />

              {/* For Sales Order Route */}
              <Route path="/salesorderhomepage" element={<SalesOrderHomePage />} />
              <Route path="/salesorderlist" element={<SalesOrderList />} />

               {/* For Sales Chart Route */}
               <Route path="/salescharthomepage" element={<SalesChartHomePage />} />
              {/* <Route path="/salesorderlist" element={<SalesOrderList />} /> */}

            </Routes>
          </>
        )}
      </div>
    </Router>
  );
}

export default App;
