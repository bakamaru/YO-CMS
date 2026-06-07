import React, { useState } from 'react';
import { useGetEventsQuery, useCreateEventMutation, useUpdateEventMutation, useDeleteEventMutation, useToggleEventStatusMutation, useTestTriggerEventMutation } from '../api';
import { Button, ButtonGroup, Card, CardBody, CardHeader, Form, FormGroup, Label, Input, TextArea, Modal, ModalHeader, ModalBody, ModalFooter, Table, Pagination, Badge, Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from 'reactstrap';
import { FaPlus, FaEdit, FaTrash, FaPlay, FaEye, FaSync } from 'react-icons/fa';

export const NotificationEventsPage: React.FC = () => {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [keyword, setKeyword] = useState('');
  const [moduleName, setModuleName] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState<boolean | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingEvent, setEditingEvent] = useState<any>(null);
  const [formData, setFormData] = useState({
    eventKey: '',
    name: '',
    moduleName: '',
    description: '',
    samplePayloadJson: '{}',
    isActive: true,
  });

  const { data, isLoading, refetch } = useGetEventsQuery({
    page,
    pageSize,
    keyword,
    moduleName,
    isActive: isActiveFilter,
  });

  const [createEvent] = useCreateEventMutation();
  const [updateEvent] = useUpdateEventMutation();
  const [deleteEvent] = useDeleteEventMutation();
  const [toggleStatus] = useToggleEventStatusMutation();
  const [testTrigger] = useTestTriggerEventMutation();

  const handleOpenModal = (event?: any) => {
    if (event) {
      setEditingEvent(event);
      setFormData(event);
    } else {
      setEditingEvent(null);
      setFormData({ eventKey: '', name: '', moduleName: '', description: '', samplePayloadJson: '{}', isActive: true });
    }
    setModalOpen(true);
  };

  const handleCloseModal = () => {
    setModalOpen(false);
    setEditingEvent(null);
    setFormData({ eventKey: '', name: '', moduleName: '', description: '', samplePayloadJson: '{}', isActive: true });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingEvent) {
        await updateEvent({ id: editingEvent.notificationEventId, body: formData }).unwrap();
      } else {
        await createEvent(formData).unwrap();
      }
      handleCloseModal();
      refetch();
    } catch (err) {
      console.error(err);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this event?')) return;
    try {
      await deleteEvent(id).unwrap();
      refetch();
    } catch (err) {
      console.error(err);
    }
  };

  const handleToggleStatus = async (id: number, isActive: boolean) => {
    try {
      await toggleStatus({ id, isActive }).unwrap();
      refetch();
    } catch (err) {
      console.error(err);
    }
  };

  const handleTestTrigger = async (id: number) => {
    const payload = window.prompt('Enter test payload JSON:', '{}');
    if (!payload) return;
    try {
      await testTrigger({ id, payload: JSON.parse(payload) }).unwrap();
      alert('Test trigger executed');
    } catch (err) {
      console.error(err);
      alert('Test trigger failed');
    }
  };

  if (isLoading) return <div className="text-center py-5">Loading...</div>;

  return (
    <div className="page-content">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Notification Events</h1>
        <Button color="primary" onClick={() => handleOpenModal()}>
          <FaPlus /> Add Event
        </Button>
      </div>

      <Card className="mb-4">
        <CardBody>
          <Form inline className="row g-3">
            <FormGroup className="col-md-3">
              <Input
                type="text"
                placeholder="Search..."
                value={keyword}
                onChange={(e) => setKeyword(e.target.value)}
              />
            </FormGroup>
            <FormGroup className="col-md-2">
              <Input
                type="text"
                placeholder="Module"
                value={moduleName}
                onChange={(e) => setModuleName(e.target.value)}
              />
            </FormGroup>
            <FormGroup className="col-md-2">
              <select className="form-select" value={isActiveFilter === null ? '' : String(isActiveFilter)} onChange={(e) => setIsActiveFilter(e.target.value === '' ? null : e.target.value === 'true')}>
                <option value="">All Status</option>
                <option value="true">Active</option>
                <option value="false">Inactive</option>
              </select>
            </FormGroup>
          </Form>
        </CardBody>
      </Card>

      <Card>
        <Table responsive>
          <thead>
            <tr>
              <th>Event Key</th>
              <th>Name</th>
              <th>Module</th>
              <th>Status</th>
              <th>Rules</th>
              <th>Last Triggered</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {data?.items.map((event) => (
              <tr key={event.notificationEventId}>
                <td><code>{event.eventKey}</code></td>
                <td>{event.name}</td>
                <td>{event.moduleName}</td>
                <td><Badge color={event.isActive ? 'success' : 'secondary'}>{event.isActive ? 'Active' : 'Inactive'}</Badge></td>
                <td>{event.rulesCount}</td>
                <td>{event.lastTriggeredOn ? new Date(event.lastTriggeredOn).toLocaleString() : 'Never'}</td>
                <td>
                  <Dropdown>
                    <DropdownToggle caret>
                      <FaEllipsisV />
                    </DropdownToggle>
                    <DropdownMenu>
                      <DropdownItem onClick={() => handleOpenModal(event)}>
                        <FaEye /> View
                      </DropdownItem>
                      <DropdownItem onClick={() => handleOpenModal(event)}>
                        <FaEdit /> Edit
                      </DropdownItem>
                      <DropdownItem onClick={() => handleToggleStatus(event.notificationEventId, !event.isActive)}>
                        <FaSync /> {event.isActive ? 'Disable' : 'Enable'}
                      </DropdownItem>
                      <DropdownItem onClick={() => handleTestTrigger(event.notificationEventId)}>
                        <FaPlay /> Test Trigger
                      </DropdownItem>
                      <DropdownItem divider />
                      <DropdownItem onClick={() => handleDelete(event.notificationEventId)} className="text-danger">
                        <FaTrash /> Delete
                      </DropdownItem>
                    </DropdownMenu>
                  </Dropdown>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>

        {data && data.totalCount > pageSize && (
          <Pagination>
            <Button onClick={() => setPage(p => p - 1)} disabled={page === 1}>&laquo;</Button>
            {Array.from({ length: Math.ceil(data.totalCount / pageSize) }, (_, i) => i + 1).map((p) => (
              <Button key={p} active={p === page} onClick={() => setPage(p)}>{p}</Button>
            ))}
            <Button onClick={() => setPage(p => p + 1)} disabled={page === Math.ceil(data.totalCount / pageSize)}>&raquo;</Button>
          </Pagination>
        )}
      </Card>

      <Modal isOpen={modalOpen} toggle={handleCloseModal} size="lg">
        <ModalHeader toggle={handleCloseModal}>{editingEvent ? 'Edit Event' : 'Add Event'}</ModalHeader>
        <Form onSubmit={handleSubmit}>
          <ModalBody>
            <FormGroup>
              <Label>Event Key *</Label>
              <Input value={formData.eventKey} onChange={(e) => setFormData({...formData, eventKey: e.target.value})} placeholder="User.SignupCompleted" disabled={!!editingEvent} />
            </FormGroup>
            <FormGroup>
              <Label>Name *</Label>
              <Input value={formData.name} onChange={(e) => setFormData({...formData, name: e.target.value})} placeholder="User Signup Completed" />
            </FormGroup>
            <FormGroup>
              <Label>Module</Label>
              <Input value={formData.moduleName} onChange={(e) => setFormData({...formData, moduleName: e.target.value})} placeholder="User" />
            </FormGroup>
            <FormGroup>
              <Label>Description</Label>
              <TextArea value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})} rows={3} />
            </FormGroup>
            <FormGroup>
              <Label>Sample Payload JSON</Label>
              <TextArea value={formData.samplePayloadJson} onChange={(e) => setFormData({...formData, samplePayloadJson: e.target.value})} rows={5} />
            </FormGroup>
            <FormGroup check>
              <Input type="checkbox" id="isActive" checked={formData.isActive} onChange={(e) => setFormData({...formData, isActive: e.target.checked})} />
              <Label for="isActive">Active</Label>
            </FormGroup>
          </ModalBody>
          <ModalFooter>
            <Button color="secondary" onClick={handleCloseModal}>Cancel</Button>
            <Button color="primary" type="submit">{editingEvent ? 'Update' : 'Create'}</Button>
          </ModalFooter>
        </Form>
      </Modal>
    </div>
  );
};