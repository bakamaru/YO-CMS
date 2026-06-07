-- Insert missing ClientForm locale keys for en-us
-- Run this on your database server (192.168.1.187, database yoframework)

INSERT INTO LocaleResource (Culture, Name, Value, GroupName)
SELECT 'en-us', k, v, 'ClientForm'
FROM (VALUES
    -- Templates
    ('ClientForm.Empty', 'Empty Client'),
    ('ClientForm.EmptyDesc', 'Start from scratch with a clean slate.'),
    ('ClientForm.SPA', 'Single Page Application'),
    ('ClientForm.SPADesc', 'React, Angular, Vue, Blazor WASM. Uses PKCE.'),
    ('ClientForm.WebApp', 'Web Application'),
    ('ClientForm.WebAppDesc', 'Server-side apps (MVC, Next.js). Uses Client Secret.'),
    ('ClientForm.M2M', 'Service / Machine-to-Machine'),
    ('ClientForm.M2MDesc', 'Daemon services, cron jobs. client_credentials flow.'),
    ('ClientForm.Mobile', 'Mobile / Native'),
    ('ClientForm.MobileDesc', 'iOS, Android. Uses PKCE and custom schemes.'),
    ('ClientForm.Device', 'Device / IoT'),
    ('ClientForm.DeviceDesc', 'Devices with limited input capabilities.'),
    
    -- Steps
    ('ClientForm.StepType', 'Type'),
    ('ClientForm.StepBasics', 'Basics'),
    ('ClientForm.StepSettings', 'Settings'),
    ('ClientForm.StepReview', 'Review'),
    
    -- Basic Info
    ('ClientForm.ClientId', 'Client ID'),
    ('ClientForm.ClientIdRequired', 'Client ID is required'),
    ('ClientForm.ClientIdPattern', 'Only alphanumeric characters, dashes, and underscores allowed'),
    ('ClientForm.ClientIdPlaceholder', 'e.g. my-awesome-app'),
    ('ClientForm.ClientIdLocked', 'Client ID cannot be changed after creation'),
    ('ClientForm.DisplayName', 'Display Name'),
    ('ClientForm.DisplayNameRequired', 'Display name is required'),
    ('ClientForm.DisplayNamePlaceholder', 'e.g. My Awesome App'),
    
    -- Settings
    ('ClientForm.DetailedConfig', 'Detailed Configuration'),
    ('ClientForm.ClientSecret', 'Client Secret'),
    ('ClientForm.ClientSecretEditPlaceholder', 'Leave empty to keep unchanged'),
    ('ClientForm.ClientSecretNewPlaceholder', 'Leave empty to generate automatically'),
    ('ClientForm.ClientSecretHint', 'Required for confidential clients (Web Apps, APIs, etc).'),
    ('ClientForm.ClientType', 'Client Type'),
    ('ClientForm.PublicClientType', 'Public (SPA, Mobile)'),
    ('ClientForm.ConfidentialClientType', 'Confidential (Web App, M2M)'),
    ('ClientForm.ConsentType', 'Consent Type'),
    ('ClientForm.ConsentExplicit', 'Explicit (User must approve)'),
    ('ClientForm.ConsentExternal', 'External'),
    ('ClientForm.ConsentImplicit', 'Implicit'),
    ('ClientForm.ConsentSystematic', 'Systematic (Trust implicitly)'),
    ('ClientForm.RedirectURIs', 'Redirect URIs'),
    ('ClientForm.RedirectURIPlaceholder', 'https://app.com/callback'),
    ('ClientForm.PostLogoutURIs', 'Post Logout Redirect URIs'),
    ('ClientForm.PostLogoutURIPlaceholder', 'https://app.com/logout-callback'),
    ('ClientForm.Permissions', 'Permissions & Capabilities'),
    
    -- Review
    ('ClientForm.ReviewTitle', 'Review Client Configuration'),
    ('ClientForm.Type', 'Type'),
    ('ClientForm.Consent', 'Consent'),
    ('ClientForm.NoneConfigured', 'None configured'),
    ('ClientForm.PermissionsCount', 'Permissions'),
    
    -- Wizard
    ('ClientForm.SelectTemplate', 'Select a Template'),
    ('ClientForm.BasicInfo', 'Basic Information'),
    ('ClientForm.EditTitle', 'Edit Client'),
    ('ClientForm.AddTitle', 'Create New Client'),
    ('ClientForm.StepOf', 'Step {{current}} of {{total}}'),
    ('ClientForm.Previous', 'Back'),
    ('ClientForm.Next', 'Next'),
    ('ClientForm.ConfirmSave', 'Confirm & Save'),
    ('ClientForm.CreateSuccess', 'Client created successfully!'),
    ('ClientForm.UpdateSuccess', 'Client updated successfully!'),
    ('ClientForm.SaveError', 'An error occurred while saving the client.'),
    ('ClientForm.NameValidateFailed', 'Failed to validate client name.'),
    ('ClientForm.NameExists', 'Client with this name already exists.')
) AS t(k, v)
WHERE NOT EXISTS (SELECT 1 FROM LocaleResource WHERE Culture='en-us' AND Name=t.k AND GroupName='ClientForm');

-- Trigger resource rebuild (optional, will happen on next API call)
-- The ResourceBuilder.Build() is called automatically after saveMissing