/** Gateway route prefixes (YARP public paths). */
export const GATEWAY_PATH_PREFIXES = [
  '/users',
  '/projects',
  '/tasks',
  '/teams',
  '/clients',
  '/sprints',
  '/subtasks',
  '/comments',
  '/notifications',
  '/activities',
  '/attachments',
  '/analytics',
  '/issues',
  '/filters',
  '/links',
  '/versions',
  '/hubs',
] as const;

export const ANONYMOUS_GATEWAY_PATHS = new Set([
  '/users/authenticate',
  '/users/register',
  '/users/refresh',
]);
