import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Space,
  Tag,
  Modal,
  Form,
  Input,
  Select,
  message,
  Popconfirm,
  Switch,
  Typography,
} from 'antd';
import {
  UserAddOutlined,
  EditOutlined,
  DeleteOutlined,
  LockOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from '@ant-design/icons';
import { authService } from '../services/api';
import { User, CreateUserRequest, UpdateUserRequest } from '../types/auth';

const { Title } = Typography;
const { Option } = Select;

const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [includeInactive, setIncludeInactive] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    loadUsers();
  }, [includeInactive]);

  const loadUsers = async () => {
    setLoading(true);
    try {
      const response = await authService.getAllUsers(includeInactive);
      setUsers(response.users);
    } catch (error: any) {
      message.error('Failed to load users');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingUser(null);
    form.resetFields();
    setModalVisible(true);
  };

  const handleEdit = (user: User) => {
    setEditingUser(user);
    form.setFieldsValue({
      email: user.email,
      role: user.role,
      notes: user.notes,
    });
    setModalVisible(true);
  };

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields();

      if (editingUser) {
        // Update user
        const updateRequest: UpdateUserRequest = {
          email: values.email,
          role: values.role,
          notes: values.notes,
        };
        await authService.updateUser(editingUser.id, updateRequest);
        message.success('User updated successfully');
      } else {
        // Create user
        const createRequest: CreateUserRequest = {
          username: values.username,
          email: values.email,
          password: values.password,
          role: values.role,
          notes: values.notes,
        };
        await authService.createUser(createRequest);
        message.success('User created successfully');
      }

      setModalVisible(false);
      form.resetFields();
      loadUsers();
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Operation failed');
    }
  };

  const handleDelete = async (userId: number) => {
    try {
      await authService.deleteUser(userId);
      message.success('User deleted successfully');
      loadUsers();
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Failed to delete user');
    }
  };

  const handleToggleActive = async (user: User) => {
    try {
      if (user.isActive) {
        await authService.deactivateUser(user.id);
        message.success('User deactivated successfully');
      } else {
        await authService.activateUser(user.id);
        message.success('User activated successfully');
      }
      loadUsers();
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Operation failed');
    }
  };

  const handleResetPassword = (user: User) => {
    Modal.confirm({
      title: 'Reset Password',
      content: (
        <Form
          id="reset-password-form"
          onFinish={async (values) => {
            try {
              await authService.resetPassword({
                userId: user.id,
                newPassword: values.newPassword,
              });
              message.success('Password reset successfully');
              Modal.destroyAll();
            } catch (error: any) {
              message.error('Failed to reset password');
            }
          }}
        >
          <Form.Item
            name="newPassword"
            label="New Password"
            rules={[
              { required: true, message: 'Please input new password' },
              { min: 6, message: 'Password must be at least 6 characters' },
            ]}
          >
            <Input.Password />
          </Form.Item>
        </Form>
      ),
      okText: 'Reset',
      onOk: () => {
        const formElement = document.getElementById('reset-password-form') as HTMLFormElement;
        formElement?.dispatchEvent(new Event('submit', { cancelable: true, bubbles: true }));
      },
    });
  };

  const columns = [
    {
      title: 'Username',
      dataIndex: 'username',
      key: 'username',
      render: (text: string, record: User) => (
        <Space>
          <span>{text}</span>
          {!record.isActive && <Tag color="red">Inactive</Tag>}
        </Space>
      ),
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
    },
    {
      title: 'Role',
      dataIndex: 'role',
      key: 'role',
      render: (role: string) => (
        <Tag color={role === 'Admin' ? 'blue' : 'green'}>{role}</Tag>
      ),
    },
    {
      title: 'Created Date',
      dataIndex: 'createdDate',
      key: 'createdDate',
      render: (date: string) => new Date(date).toLocaleDateString(),
    },
    {
      title: 'Last Login',
      dataIndex: 'lastLoginDate',
      key: 'lastLoginDate',
      render: (date?: string) => date ? new Date(date).toLocaleString() : 'Never',
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: User) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
            size="small"
          >
            Edit
          </Button>
          <Button
            type="link"
            icon={<LockOutlined />}
            onClick={() => handleResetPassword(record)}
            size="small"
          >
            Reset Password
          </Button>
          <Button
            type="link"
            icon={record.isActive ? <StopOutlined /> : <CheckCircleOutlined />}
            onClick={() => handleToggleActive(record)}
            size="small"
            danger={record.isActive}
          >
            {record.isActive ? 'Deactivate' : 'Activate'}
          </Button>
          <Popconfirm
            title="Are you sure you want to delete this user?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="link"
              danger
              icon={<DeleteOutlined />}
              size="small"
            >
              Delete
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Title level={2}>User Management</Title>

      <Card>
        <Space style={{ marginBottom: 16, justifyContent: 'space-between', width: '100%' }}>
          <Button
            type="primary"
            icon={<UserAddOutlined />}
            onClick={handleCreate}
          >
            Create User
          </Button>
          <Space>
            <span>Show Inactive:</span>
            <Switch checked={includeInactive} onChange={setIncludeInactive} />
          </Space>
        </Space>

        <Table
          columns={columns}
          dataSource={users}
          rowKey="id"
          loading={loading}
          pagination={{ pageSize: 10 }}
        />
      </Card>

      <Modal
        title={editingUser ? 'Edit User' : 'Create User'}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => {
          setModalVisible(false);
          form.resetFields();
        }}
        width={600}
      >
        <Form form={form} layout="vertical">
          {!editingUser && (
            <>
              <Form.Item
                name="username"
                label="Username"
                rules={[
                  { required: true, message: 'Please input username' },
                  { min: 3, message: 'Username must be at least 3 characters' },
                ]}
              >
                <Input placeholder="Enter username" />
              </Form.Item>

              <Form.Item
                name="password"
                label="Password"
                rules={[
                  { required: true, message: 'Please input password' },
                  { min: 6, message: 'Password must be at least 6 characters' },
                ]}
              >
                <Input.Password placeholder="Enter password" />
              </Form.Item>
            </>
          )}

          <Form.Item
            name="email"
            label="Email"
            rules={[
              { required: true, message: 'Please input email' },
              { type: 'email', message: 'Please enter a valid email' },
            ]}
          >
            <Input placeholder="Enter email" />
          </Form.Item>

          <Form.Item
            name="role"
            label="Role"
            rules={[{ required: true, message: 'Please select role' }]}
          >
            <Select placeholder="Select role">
              <Option value="Admin">Admin</Option>
              <Option value="ServiceUser">Service User</Option>
            </Select>
          </Form.Item>

          <Form.Item name="notes" label="Notes">
            <Input.TextArea rows={3} placeholder="Optional notes" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default UserManagement;
