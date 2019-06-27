import React from 'react';
import { Link } from 'react-router-dom';
import { Glyphicon, Nav, Navbar, NavItem } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';
import './NavMenu.css';

export default props => (
  <Navbar inverse fixedTop fluid collapseOnSelect>
    <Navbar.Header>
      <Navbar.Brand>
        <Link to={'/'}>ShowTime</Link>
      </Navbar.Brand>
      <Navbar.Toggle />
    </Navbar.Header>
    <Navbar.Collapse>
      <Nav>
        <LinkContainer to={'/'} exact>
          <NavItem>
            <Glyphicon glyph='home' /> Home
          </NavItem>
        </LinkContainer>
        {/*
        <LinkContainer to={'/counter'}>
          <NavItem>
            <Glyphicon glyph='education' /> Counter
          </NavItem>
        </LinkContainer>
        <LinkContainer to={'/fetchdata'}>
          <NavItem>
            <Glyphicon glyph='th-list' /> Fetch data
          </NavItem>
        </LinkContainer>
        */}
        <LinkContainer to={'/train'}>
          <NavItem>
            <Glyphicon glyph='education' /> Обучение модели
          </NavItem>
        </LinkContainer>
        <LinkContainer to={'/predict'}>
          <NavItem>
            <Glyphicon glyph='eye-open' /> Классификация<br/>изображений
          </NavItem>
        </LinkContainer>
        <LinkContainer to={'/transfer'}>
          <NavItem>
            <Glyphicon glyph='picture' /> Перенос<br/>обучения
          </NavItem>
        </LinkContainer>
      </Nav>
    </Navbar.Collapse>
  </Navbar>
);
