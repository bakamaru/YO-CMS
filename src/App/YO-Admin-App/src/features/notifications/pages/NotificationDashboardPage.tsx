import React from 'react';
import { useGetDashboardQuery } from '../api';
import { Card, CardBody, Row, Col, Badge, Table } from 'reactstrap';

export const NotificationDashboardPage: React.FC = () => {
  const { data, isLoading, error, refetch } = useGetDashboardQuery();

  if (isLoading) return <div className="text-center py-5">Loading...</div>;
  if (error) return <div className="text-danger">Error loading dashboard</div>;
  if (!data) return null;

  const statCards = [
    { title: 'Total Events', value: data.totalEvents, color: 'primary' },
    { title: 'Active Rules', value: data.activeRules, color: 'success' },
    { title: 'Pending Outbox', value: data.pendingOutbox, color: 'warning' },
    { title: 'Failed Notifications', value: data.failedNotifications, color: 'danger' },
    { title: 'Emails Sent Today', value: data.emailsSentToday, color: 'info' },
    { title: 'In-App Sent Today', value: data.inAppSentToday, color: 'secondary' },
    { title: 'Dead Letter Events', value: data.deadLetterCount, color: 'dark' },
  ];

  return (
    <div className="page-content">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Notification Dashboard</h1>
        <button className="btn btn-outline-primary" onClick={() => refetch()}>
          Refresh
        </button>
      </div>

      <Row className="mb-4">
        {statCards.map((stat, index) => (
          <Col key={index} xs="12" sm="6" md="4" lg="3">
            <Card className="h-100">
              <CardBody className="text-center">
                <h3 className={`text-${stat.color}`}>{stat.value}</h3>
                <p className="text-muted mb-0">{stat.title}</p>
              </CardBody>
            </Card>
          </Col>
        ))}
      </Row>

      <Row>
        <Col md="6">
          <Card>
            <CardBody>
              <h5>Recent Events</h5>
              <Table responsive>
                <thead>
                  <tr>
                    <th>Event Key</th>
                    <th>Name</th>
                    <th>Triggers</th>
                    <th>Last Triggered</th>
                  </tr>
                </thead>
                <tbody>
                  {data.recentEvents.slice(0, 5).map((event) => (
                    <tr key={event.eventKey}>
                      <td><code>{event.eventKey}</code></td>
                      <td>{event.name}</td>
                      <td>{event.triggerCount}</td>
                      <td>{event.lastTriggeredOn ? new Date(event.lastTriggeredOn).toLocaleString() : 'Never'}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </CardBody>
          </Card>
        </Col>

        <Col md="6">
          <Card>
            <CardBody>
              <h5>Recent Failed Sends</h5>
              <Table responsive>
                <thead>
                  <tr>
                    <th>Event</th>
                    <th>Channel</th>
                    <th>Receiver</th>
                    <th>Error</th>
                  </tr>
                </thead>
                <tbody>
                  {data.recentFailedSends.slice(0, 5).map((fail, i) => (
                    <tr key={i}>
                      <td><code>{fail.eventKey}</code></td>
                      <td><Badge color="secondary">{fail.channel}</Badge></td>
                      <td>{fail.receiver}</td>
                      <td className="text-danger text-truncate" style={{ maxWidth: '200px' }}>{fail.errorMessage}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </CardBody>
          </Card>
        </Col>
      </Row>

      <Row className="mt-4">
        <Col>
          <Card>
            <CardBody>
              <h5>Top Triggered Events</h5>
              <Table responsive>
                <thead>
                  <tr>
                    <th>Event Key</th>
                    <th>Name</th>
                    <th>Trigger Count</th>
                  </tr>
                </thead>
                <tbody>
                  {data.topTriggeredEvents.map((event, i) => (
                    <tr key={i}>
                      <td><code>{event.eventKey}</code></td>
                      <td>{event.name}</td>
                      <td>{event.triggerCount}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
};