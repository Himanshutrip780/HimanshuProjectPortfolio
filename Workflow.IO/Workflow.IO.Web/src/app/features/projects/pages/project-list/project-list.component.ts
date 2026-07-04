import { Component, inject, OnInit, signal, computed, ViewChild, ElementRef, effect } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import {
  FormControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { read, utils } from 'xlsx';

export interface ImportTaskDto {
  title: string;
  description?: string;
  priority: number;
  issueType: number;
  storyPoints?: number;
  dueDate?: string;
}
import { Router, RouterLink } from '@angular/router';

import {
  ProjectResponse,
  ProjectType,
  ProjectStatus,
  ProjectMember,
} from '../../../../core/models/project.models';
import { TaskResponse } from '../../../../core/models/task.models';
import { ActivityRecord } from '../../../../core/models/activity.models';
import { UserDto } from '../../../../core/models/user.models';
import { AuthService } from '../../../../core/services/auth.service';
import { UserService } from '../../../../core/services/user.service';
import { TaskService } from '../../../tasks/services/task.service';
import { ActivityService } from '../../../../core/services/activity.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { ProjectService } from '../../services/project.service';
import { BackButtonService } from '../../../../core/services/back-button.service';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, PaginationComponent],
  templateUrl: './project-list.component.html',
  styles: `
    .page-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .page-header h1 {
      font-size: 1.5rem;
      font-weight: 700;
      letter-spacing: -0.025em;
      color: var(--text-primary);
      margin: 0;
    }

    .page-header .subtitle {
      font-size: 0.85rem;
      color: var(--text-secondary);
      margin-top: 0.15rem;
    }

    .header-actions {
      display: flex;
      gap: 0.75rem;
    }

    .btn {
      font-size: 0.8rem;
      font-weight: 600;
      padding: 0.45rem 0.9rem;
      border-radius: var(--radius-md);
    }

    .btn-icon {
      font-size: 1.1rem;
    }

    /* Stats summary grid */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1rem;
    }

    .stat-card {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1rem;
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      box-shadow: var(--shadow-sm);
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    }

    .stat-card:hover {
      border-color: var(--primary-color);
      box-shadow: var(--shadow-hover);
    }

    .stat-icon-wrapper {
      width: 36px;
      height: 36px;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .stat-icon-wrapper span {
      font-size: 1.25rem;
    }

    .folder-icon { background-color: rgba(99, 102, 241, 0.1); color: #6366f1; }
    .active-icon { background-color: rgba(59, 130, 246, 0.1); color: #3b82f6; }
    .completed-icon { background-color: rgba(16, 185, 129, 0.1); color: #10b981; }
    .tasks-icon { background-color: rgba(245, 158, 11, 0.1); color: #f59e0b; }

    .stat-details {
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
      flex: 1;
    }

    .stat-label {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .stat-value {
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--text-primary);
      line-height: 1.1;
    }

    .stat-trend {
      display: inline-flex;
      align-items: center;
      gap: 0.15rem;
      font-size: 0.7rem;
      font-weight: 700;
      margin-top: 0.25rem;
    }

    .trend-up {
      color: #10b981;
    }

    .trend-up span {
      font-size: 0.85rem;
    }

    .trend-text {
      color: var(--text-muted);
      font-weight: 500;
      margin-left: 0.1rem;
    }

    /* Main Grid Layout */
    .dashboard-columns {
      display: grid;
      grid-template-columns: 1fr 300px;
      gap: 1.5rem;
    }

    @media (max-width: 1024px) {
      .dashboard-columns {
        grid-template-columns: 1fr;
      }
    }

    .projects-main {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      min-width: 0;
    }

    /* Toolbar styling */
    .toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.5rem 0;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .toolbar-left {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .toolbar-title {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .toolbar-badge {
      background-color: var(--bg-hover);
      color: var(--text-secondary);
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.05rem 0.45rem;
      border-radius: 9999px;
      border: 1px solid var(--border-color);
    }

    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      flex-wrap: wrap;
    }

    .search-box {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.35rem 0.65rem;
      width: 200px;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    }

    .search-box:focus-within {
      border-color: var(--primary-color);
      box-shadow: 0 0 0 2px var(--primary-glow);
    }

    .search-box span {
      font-size: 1rem;
      color: var(--text-muted);
    }

    .search-box input {
      background: transparent;
      border: none;
      outline: none;
      font-size: 0.8rem;
      color: var(--text-primary);
      width: 100%;
    }

    .search-box input::placeholder {
      color: var(--text-muted);
    }

    .btn-toolbar {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      color: var(--text-secondary);
      font-size: 0.8rem;
      font-weight: 600;
      padding: 0.35rem 0.65rem;
      border-radius: var(--radius-md);
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      cursor: pointer;
      transition: all var(--transition-fast);
    }

    .btn-toolbar:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
      border-color: var(--text-muted);
    }

    .btn-toolbar span {
      font-size: 1.05rem;
    }

    .select-toolbar {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      color: var(--text-secondary);
      font-size: 0.8rem;
      font-weight: 600;
      padding: 0.35rem 1.5rem 0.35rem 0.65rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      outline: none;
      transition: all var(--transition-fast);
      appearance: none;
      background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23a1a1aa' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E");
      background-repeat: no-repeat;
      background-position: right 0.4rem center;
      background-size: 1rem;
    }

    .select-toolbar:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
      border-color: var(--text-muted);
    }

    .trend-down {
      color: #ef4444;
    }

    .view-toggles {
      display: flex;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      overflow: hidden;
      background-color: var(--bg-card);
    }

    .btn-toggle {
      background: transparent;
      border: none;
      color: var(--text-muted);
      padding: 0.35rem;
      display: flex;
      align-items: center;
      cursor: pointer;
      transition: all var(--transition-fast);
    }

    .btn-toggle.active {
      background-color: var(--bg-hover);
      color: var(--primary-color);
    }

    .btn-toggle span {
      font-size: 1.1rem;
    }

    /* Projects Grid */
    .projects-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1rem;
    }

    .project-card {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      cursor: pointer;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast), transform var(--transition-fast);
      box-shadow: var(--shadow-sm);
      position: relative;
    }

    .project-card:hover {
      border-color: var(--primary-color);
      box-shadow: var(--shadow-hover);
      transform: translateY(-2px);
    }

    .project-card-header {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      width: 100%;
    }

    .project-avatar {
      width: 32px;
      height: 32px;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      color: #fff;
      font-weight: 700;
      font-size: 0.85rem;
      text-transform: uppercase;
    }

    .project-title-area {
      display: flex;
      flex-direction: column;
      flex: 1;
      min-width: 0;
    }

    .project-title-area h3 {
      font-size: 0.9rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .project-meta-text {
      font-size: 0.7rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .btn-card-menu {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      display: flex;
      align-items: center;
      padding: 0.15rem;
      border-radius: var(--radius-sm);
    }

    .btn-card-menu:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .project-description {
      font-size: 0.75rem;
      color: var(--text-secondary);
      line-height: 1.45;
      margin: 0;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      text-overflow: ellipsis;
      min-height: 38px;
    }

    /* Progress bar */
    .project-progress-container {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .progress-bar-wrapper {
      flex: 1;
      height: 4px;
      background-color: var(--bg-hover);
      border-radius: 99px;
      overflow: hidden;
    }

    .progress-fill {
      height: 100%;
      background-color: var(--primary-color);
      border-radius: 99px;
    }

    .progress-percent-text {
      font-size: 0.7rem;
      font-weight: 700;
      color: var(--text-primary);
      width: 28px;
      text-align: right;
    }

    .project-card-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: 0.75rem;
      border-top: 1px solid var(--border-color);
    }

    /* Avatar stack */
    .members-avatars {
      display: flex;
      align-items: center;
    }

    .avatar-circle {
      width: 22px;
      height: 22px;
      border-radius: 50%;
      border: 1.5px solid var(--bg-card);
      background-color: var(--bg-hover);
      color: var(--text-secondary);
      font-size: 0.6rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-left: -6px;
      object-fit: cover;
    }

    .members-avatars .avatar-circle:first-child {
      margin-left: 0;
    }

    .avatar-excess {
      width: 22px;
      height: 22px;
      border-radius: 50%;
      border: 1.5px solid var(--bg-card);
      background-color: var(--bg-hover);
      color: var(--primary-color);
      font-size: 0.6rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-left: -6px;
    }

    .footer-stats {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .stat-item {
      display: inline-flex;
      align-items: center;
      gap: 0.2rem;
      font-size: 0.7rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .stat-item span {
      font-size: 0.95rem;
      color: var(--text-muted);
    }

    .status-badge {
      font-size: 0.65rem;
      font-weight: 700;
      padding: 0.1rem 0.45rem;
      border-radius: 9999px;
      text-transform: capitalize;
    }

    .status-on-track { background-color: rgba(16, 185, 129, 0.1); color: #10b981; }
    .status-at-risk { background-color: rgba(245, 158, 11, 0.1); color: #f59e0b; }
    .status-behind { background-color: rgba(239, 68, 68, 0.1); color: #ef4444; }

    /* Right Sidebar Widgets */
    .dashboard-sidebar {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .sidebar-widget {
      background-color: var(--bg-panel);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1.15rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      box-shadow: var(--shadow-sm);
    }

    .widget-header {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 0.5rem;
    }

    .widget-header h3 {
      font-size: 0.85rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0;
      flex: 1;
    }

    .purple-text {
      color: var(--primary-color);
      font-size: 1.15rem;
    }

    .view-all-link {
      font-size: 0.7rem;
      font-weight: 600;
      color: var(--primary-color);
      text-decoration: none;
    }

    .view-all-link:hover {
      text-decoration: underline;
    }

    .quick-open-form {
      width: 100%;
    }

    .quick-open-group {
      display: flex;
      gap: 0.5rem;
      background-color: var(--bg-hover);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.25rem 0.45rem;
    }

    .quick-open-group input {
      background: transparent;
      border: none;
      outline: none;
      font-size: 0.8rem;
      color: var(--text-primary);
      flex: 1;
      min-width: 0;
    }

    .quick-open-group input::placeholder {
      color: var(--text-muted);
    }

    .btn-arrow {
      background-color: var(--primary-color);
      color: #fff;
      border: none;
      border-radius: var(--radius-sm);
      padding: 0.25rem;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: background-color var(--transition-fast);
    }

    .btn-arrow:hover:not(:disabled) {
      background-color: var(--primary-hover);
    }

    .activity-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .activity-item {
      display: flex;
      align-items: flex-start;
      gap: 0.55rem;
    }

    .activity-avatar {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      color: #fff;
      font-size: 0.65rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      text-transform: uppercase;
      object-fit: cover;
    }

    .bg-purple { background-color: #8b5cf6; }
    .bg-blue { background-color: #3b82f6; }
    .bg-green { background-color: #10b981; }
    .bg-orange { background-color: #f59e0b; }

    .activity-details {
      display: flex;
      flex-direction: column;
      gap: 0.1rem;
      min-width: 0;
    }

    .activity-text {
      font-size: 0.75rem;
      color: var(--text-primary);
      margin: 0;
      line-height: 1.35;
    }

    .activity-text strong {
      font-weight: 600;
    }

    .activity-meta {
      font-size: 0.65rem;
      color: var(--text-secondary);
    }

    /* Donut chart */
    .chart-wrapper {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .donut-chart-container {
      width: 84px;
      height: 84px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .donut-svg {
      transform: rotate(-90deg);
    }

    .chart-legend {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
      flex: 1;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.7rem;
    }

    .legend-dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
      display: inline-block;
      flex-shrink: 0;
    }

    .legend-label {
      color: var(--text-secondary);
      flex: 1;
    }

    .legend-count {
      font-weight: 700;
      color: var(--text-primary);
    }

    /* Modal dialog overlays */
    .modal-backdrop {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      background-color: rgba(9, 9, 11, 0.65);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      backdrop-filter: blur(4px);
      animation: fadeInModal 0.2s ease-out;
    }

    .modal-content {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      width: 460px;
      max-width: 90%;
      max-height: 90vh;
      overflow-y: auto;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
      box-shadow: var(--shadow-lg);
      animation: slideInModal 0.2s cubic-bezier(0.16, 1, 0.3, 1);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .modal-header h2 {
      font-size: 1.15rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0;
    }

    .btn-close-modal {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      display: flex;
      align-items: center;
      padding: 0.2rem;
      border-radius: var(--radius-md);
      transition: background-color var(--transition-fast);
    }

    .btn-close-modal:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .modal-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      margin-top: 0.5rem;
    }

    @keyframes fadeInModal {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes slideInModal {
      from { transform: scale(0.95) translateY(10px); opacity: 0; }
      to { transform: scale(1) translateY(0); opacity: 1; }
    }

    .loading-state, .error-state, .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 4rem 2rem;
      background: var(--bg-card);
      border: 1px dashed var(--border-color);
      border-radius: var(--radius-lg);
      color: var(--text-secondary);
      text-align: center;
      gap: 1rem;
    }

    .loading-state span, .error-state span, .empty-state span {
      font-size: 3rem;
      color: var(--text-muted);
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .spinner-sm {
      width: 16px;
      height: 16px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      display: inline-block;
      vertical-align: middle;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* Projects List Layout */
    .projects-list-layout {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .project-card.list-card {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 1.25rem;
      gap: 1.5rem;
      flex-wrap: wrap;
    }

    .project-card.list-card:hover {
      transform: translateX(2px);
    }

    .project-card.list-card .project-card-header {
      width: auto;
      flex: 1;
      min-width: 200px;
    }

    .project-card.list-card .project-description {
      display: block;
      flex: 1.5;
      min-width: 250px;
      margin: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      font-size: 0.75rem;
      min-height: 0;
    }

    .project-card.list-card .project-progress-container {
      width: 120px;
      margin: 0;
    }

    .project-card.list-card .project-card-footer {
      width: auto;
      margin: 0;
      gap: 1.5rem;
      padding-top: 0;
      border-top: none;
    }

    /* Import Modal Styles */
    .import-modal {
      width: 520px;
      max-width: 95%;
    }

    .template-downloads {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      background-color: var(--bg-panel);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.75rem;
      margin-bottom: 0.25rem;
    }

    .template-label {
      font-size: 0.75rem;
      font-weight: 700;
      color: var(--text-secondary);
    }

    .template-links {
      display: flex;
      gap: 0.75rem;
    }

    .btn-template-download {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--text-primary);
      text-decoration: none;
      padding: 0.35rem 0.65rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-sm);
      background-color: var(--bg-card);
      transition: all var(--transition-fast);
      flex: 1;
      justify-content: center;
    }

    .btn-template-download:hover {
      background-color: var(--bg-hover);
      border-color: var(--primary-color);
      color: var(--primary-color);
    }

    .btn-template-download span {
      font-size: 1rem;
    }

    .btn-template-download.sample {
      opacity: 0.85;
      font-size: 0.7rem;
    }

    .specs-toggle-container {
      margin: 0.25rem 0;
      display: flex;
    }

    .btn-specs-toggle {
      background: transparent;
      border: none;
      color: var(--primary-color);
      font-size: 0.75rem;
      font-weight: 700;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 0.25rem;
      padding: 0.25rem 0;
      transition: color var(--transition-fast);
    }

    .btn-specs-toggle:hover {
      color: var(--primary-hover);
    }

    .btn-specs-toggle .font-icon {
      font-size: 1.1rem;
    }

    .specs-panel-container {
      background-color: var(--bg-hover);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.5rem;
      max-height: 180px;
      overflow-y: auto;
      margin-bottom: 0.5rem;
    }

    .specs-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.7rem;
      text-align: left;
    }

    .specs-table th, .specs-table td {
      padding: 0.35rem 0.5rem;
      border-bottom: 1px solid var(--border-color);
      vertical-align: top;
    }

    .specs-table th {
      font-weight: 700;
      color: var(--text-secondary);
      background-color: var(--bg-panel);
    }

    .specs-table td code {
      background-color: var(--bg-panel);
      padding: 0.05rem 0.2rem;
      border-radius: var(--radius-sm);
      font-family: monospace;
      color: var(--primary-color);
    }

    .badge {
      display: inline-block;
      font-size: 0.6rem;
      font-weight: 700;
      padding: 0.05rem 0.35rem;
      border-radius: var(--radius-sm);
      text-transform: uppercase;
    }

    .badge-req {
      background-color: rgba(239, 68, 68, 0.1);
      color: #ef4444;
    }

    .badge-opt {
      background-color: rgba(16, 185, 129, 0.1);
      color: #10b981;
    }

    .drag-drop-zone {
      border: 2px dashed var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      cursor: pointer;
      background-color: var(--bg-card);
      transition: all var(--transition-fast);
      text-align: center;
      min-height: 140px;
      margin-bottom: 0.25rem;
    }

    .drag-drop-zone:hover, .drag-drop-zone.drag-over {
      border-color: var(--primary-color);
      background-color: rgba(99, 102, 241, 0.03);
    }

    .drag-drop-zone.file-loaded {
      border-style: solid;
      border-color: #10b981;
      background-color: rgba(16, 185, 129, 0.02);
      cursor: default;
    }

    .upload-icon {
      font-size: 2.2rem;
      color: var(--text-muted);
    }

    .upload-icon.success {
      color: #10b981;
    }

    .upload-text {
      font-size: 0.8rem;
      color: var(--text-primary);
      font-weight: 500;
    }

    .browse-link {
      color: var(--primary-color);
      font-weight: 700;
      text-decoration: underline;
    }

    .upload-subtext {
      font-size: 0.7rem;
      color: var(--text-secondary);
    }

    .btn-change-file {
      margin-top: 0.5rem;
      font-size: 0.7rem;
      padding: 0.25rem 0.60rem;
    }
  `,
})
export class ProjectListComponent implements OnInit {
  readonly ProjectType = ProjectType;
  private readonly projectService = inject(ProjectService);
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly userService = inject(UserService);
  private readonly taskService = inject(TaskService);
  private readonly activityService = inject(ActivityService);
  private readonly backButtonService = inject(BackButtonService);

  getGreeting(): string {
    const hr = new Date().getHours();
    if (hr < 12) return 'Good morning';
    if (hr < 17) return 'Good afternoon';
    return 'Good evening';
  }

  private createModalCleanup: (() => void) | null = null;
  private readonly createModalEffect = effect(() => {
    const isOpen = this.showCreateModal();
    if (isOpen) {
      this.createModalCleanup = this.backButtonService.registerHandler(
        'CreateProjectModal',
        10,
        () => {
          this.closeCreateModal();
          return true; // Consumed
        }
      );
    } else {
      if (this.createModalCleanup) {
        this.createModalCleanup();
        this.createModalCleanup = null;
      }
    }
  });

  private importModalCleanup: (() => void) | null = null;
  private readonly importModalEffect = effect(() => {
    const isOpen = this.showImportModal();
    if (isOpen) {
      this.importModalCleanup = this.backButtonService.registerHandler(
        'ImportProjectModal',
        11,
        () => {
          this.closeImportModal();
          return true; // Consumed
        }
      );
    } else {
      if (this.importModalCleanup) {
        this.importModalCleanup();
        this.importModalCleanup = null;
      }
    }
  });

  readonly projects = signal<ProjectResponse[]>([]);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly error = signal<string | null>(null);

  readonly usersMap = this.userService.usersMap;
  readonly projectMembersMap = signal<Record<string, ProjectMember[]>>({});
  readonly projectTasksMap = signal<Record<string, TaskResponse[]>>({});
  readonly projectActivitiesMap = signal<Record<string, ActivityRecord[]>>({});

  readonly showCreateModal = signal(false);
  readonly showImportModal = signal(false);
  readonly isDragging = signal(false);
  readonly loadedFileName = signal<string | null>(null);
  readonly importedTasks = signal<ImportTaskDto[]>([]);
  readonly showSpecs = signal(false);

  readonly searchText = new FormControl('', { nonNullable: true });

  readonly createForm = this.fb.group({
    name: ['', Validators.required],
    key: [''],
    projectType: [ProjectType.Scrum, Validators.required],
    description: [''],
  });

  readonly importForm = this.fb.group({
    name: ['', Validators.required],
    key: [''],
    projectType: [ProjectType.Scrum, Validators.required],
    description: [''],
  });

  readonly keyLookup = new FormControl('', {
    nonNullable: true,
    validators: Validators.required,
  });

  readonly statusFilterControl = new FormControl('', { nonNullable: true });
  readonly typeFilterControl = new FormControl('', { nonNullable: true });
  readonly sortByControl = new FormControl('recent', { nonNullable: true });

  readonly searchTextSig = signal(this.searchText.value);
  readonly statusFilterSig = signal(this.statusFilterControl.value);
  readonly typeFilterSig = signal(this.typeFilterControl.value);
  readonly sortBySig = signal(this.sortByControl.value);
  readonly viewMode = signal<'grid' | 'list'>('grid');

  readonly currentPage = signal(1);
  readonly pageSize = signal(10);

  readonly pagedProjects = computed(() => {
    const list = this.filteredProjects();
    const startIndex = (this.currentPage() - 1) * this.pageSize();
    return list.slice(startIndex, startIndex + this.pageSize());
  });

  readonly filteredProjects = computed(() => {
    let list = [...this.projects()];
    const query = this.searchTextSig().toLowerCase().trim();

    // 1. Text Search Filter
    if (query) {
      list = list.filter((p) =>
        p.name.toLowerCase().includes(query) ||
        p.key.toLowerCase().includes(query) ||
        (p.description && p.description.toLowerCase().includes(query))
      );
    }

    // 2. Status Filter
    const statusVal = this.statusFilterSig();
    if (statusVal) {
      if (statusVal === 'Active') {
        list = list.filter((p) => p.status === ProjectStatus.Active);
      } else if (statusVal === 'Completed') {
        list = list.filter((p) => p.status === ProjectStatus.Completed);
      } else if (statusVal === 'Archived') {
        list = list.filter((p) => p.status === ProjectStatus.Archived);
      }
    }

    // 3. Type Filter
    const typeVal = this.typeFilterSig();
    if (typeVal) {
      list = list.filter((p) => p.projectType === Number(typeVal));
    }

    // 4. Sorting
    const sortBy = this.sortBySig();
    if (sortBy === 'recent') {
      list.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    } else if (sortBy === 'name') {
      list.sort((a, b) => a.name.localeCompare(b.name));
    } else if (sortBy === 'progress') {
      list.sort((a, b) => this.getProjectProgress(b.projectId) - this.getProjectProgress(a.projectId));
    }

    return list;
  });

  ngOnInit(): void {
    this.loadUsers();
    this.loadProjects();

    this.searchText.valueChanges.subscribe(val => {
      this.searchTextSig.set(val);
      this.currentPage.set(1);
    });
    this.statusFilterControl.valueChanges.subscribe(val => {
      this.statusFilterSig.set(val || '');
      this.currentPage.set(1);
    });
    this.typeFilterControl.valueChanges.subscribe(val => {
      this.typeFilterSig.set(val || '');
      this.currentPage.set(1);
    });
    this.sortByControl.valueChanges.subscribe(val => {
      this.sortBySig.set(val || 'recent');
      this.currentPage.set(1);
    });
  }

  loadUsers(): void {
    this.userService.loadAllUsers();
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  openByKey(): void {
    const key = this.keyLookup.value.trim().toUpperCase();
    if (!key) {
      return;
    }

    this.projectService.getProjectByKey(key).subscribe({
      next: (project) =>
        void this.router.navigate(['/projects', project.projectId, 'tasks']),
      error: (err) => this.error.set(getApiErrorMessage(err)),
    });
  }

  projectTypeLabel(type: ProjectType): string {
    return ProjectType[type] ?? String(type);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
    this.createForm.reset({ projectType: ProjectType.Scrum });
  }

  createProject(): void {
    if (this.createForm.invalid) {
      return;
    }

    this.creating.set(true);
    const raw = this.createForm.getRawValue();
    this.projectService
      .createProject({
        name: raw.name,
        key: raw.key || null,
        projectType: Number(raw.projectType) as ProjectType,
        description: raw.description || null,
      })
      .subscribe({
        next: (project) => {
          this.creating.set(false);
          this.closeCreateModal();
          void this.router.navigate(['/projects', project.projectId, 'tasks']);
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.creating.set(false);
        },
      });
  }

  private loadProjects(): void {
    this.projectService.getMyProjects().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loading.set(false);
        for (const p of projects) {
          this.loadProjectData(p.projectId);
        }
      },
      error: (err) => {
        this.error.set(getApiErrorMessage(err));
        this.loading.set(false);
      },
    });
  }

  private loadProjectData(projectId: string): void {
    this.projectService.getMembers(projectId).subscribe({
      next: (members) => {
        this.projectMembersMap.update((prev) => ({
          ...prev,
          [projectId]: members,
        }));
      },
    });

    this.taskService.getProjectTasks(projectId).subscribe({
      next: (tasks) => {
        this.projectTasksMap.update((prev) => ({
          ...prev,
          [projectId]: tasks,
        }));
      },
    });

    this.activityService.getProjectActivities(projectId, 5).subscribe({
      next: (activities) => {
        this.projectActivitiesMap.update((prev) => ({
          ...prev,
          [projectId]: activities,
        }));
      },
    });
  }

  /* Dynamic UI stats & indicators based on project details */

  getProjectColor(name: string): string {
    const colors = [
      '#6366f1, #8b5cf6', // Purple to violet
      '#3b82f6, #06b6d4', // Blue to cyan
      '#ec4899, #f43f5e', // Pink to rose
      '#10b981, #059669', // Emerald green
      '#f59e0b, #d97706', // Yellow to orange
    ];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }

  getProjectProgress(projectId: string): number {
    const tasks = this.projectTasksMap()[projectId] || [];
    if (tasks.length === 0) {
      return 0;
    }
    const completed = tasks.filter((t) => t.status === 4).length; // 4 = Done
    return Math.round((completed * 100) / tasks.length);
  }

  getProjectTasksFraction(projectId: string): string {
    const tasks = this.projectTasksMap()[projectId] || [];
    const completed = tasks.filter((t) => t.status === 4).length;
    return `${completed}/${tasks.length}`;
  }

  getProjectDueDate(projectId: string): string {
    const tasks = this.projectTasksMap()[projectId] || [];
    const activeTasksWithDueDate = tasks.filter((t) => t.dueDate && t.status !== 4);
    if (activeTasksWithDueDate.length === 0) {
      return 'No due date';
    }

    const latestDate = activeTasksWithDueDate.reduce((latest, t) => {
      const d = new Date(t.dueDate!);
      return d > latest ? d : latest;
    }, new Date(0));

    if (latestDate.getTime() === 0) {
      return 'No due date';
    }

    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    return `${months[latestDate.getMonth()]} ${latestDate.getDate()}, ${latestDate.getFullYear()}`;
  }

  getProjectStatusLabel(project: ProjectResponse): string {
    if (project.status === ProjectStatus.Completed) {
      return 'Completed';
    }
    const tasks = this.projectTasksMap()[project.projectId] || [];
    if (tasks.length === 0) {
      return 'On Track';
    }

    const overdueCount = tasks.filter((t) => t.dueDate && new Date(t.dueDate) < new Date() && t.status !== 4).length;
    if (overdueCount > 0) {
      return 'Behind';
    }

    const blockedCount = tasks.filter((t) => t.status === 5).length; // 5 = Blocked
    if (blockedCount > 0) {
      return 'At Risk';
    }

    const completedCount = tasks.filter((t) => t.status === 4).length;
    if (completedCount === tasks.length) {
      return 'Completed';
    }

    return 'On Track';
  }

  getProjectStatusClass(project: ProjectResponse): string {
    const label = this.getProjectStatusLabel(project);
    if (label === 'Behind') return 'status-behind';
    if (label === 'At Risk') return 'status-at-risk';
    return 'status-on-track';
  }

  /* Compute aggregates for stats summary cards */

  getActiveProjectsCount(): number {
    return this.projects().filter((p) => {
      const status = this.getProjectStatusLabel(p);
      return status !== 'Completed' && status !== 'Archived';
    }).length;
  }

  getCompletedProjectsCount(): number {
    return this.projects().filter((p) => this.getProjectStatusLabel(p) === 'Completed').length;
  }

  getTotalTasksCount(): number {
    let count = 0;
    const taskMap = this.projectTasksMap();
    for (const projectId of Object.keys(taskMap)) {
      count += taskMap[projectId]?.length || 0;
    }
    return count;
  }

  /* User mapping lookups and time ago utility */

  getUserInitials(userId: string | null): string {
    return this.userService.getUserInitials(userId);
  }

  getUserDisplayName(userId: string | null): string {
    return this.userService.getUserDisplayName(userId);
  }

  getUserBgColor(userId: string | null): string {
    if (!userId) return '#8b5cf6';
    const colors = ['#8b5cf6', '#3b82f6', '#10b981', '#f59e0b', '#ec4899', '#f43f5e', '#06b6d4'];
    let hash = 0;
    const cleanId = userId.toLowerCase();
    for (let i = 0; i < cleanId.length; i++) {
      hash = cleanId.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }

  getUserAvatarUrl(userId: string | null): string | null {
    return this.userService.getUserAvatarUrl(userId);
  }

  formatTimeAgo(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffHr = Math.floor(diffMin / 60);
    const diffDays = Math.floor(diffHr / 24);

    if (diffDays > 0) {
      return diffDays === 1 ? '1 day ago' : `${diffDays} days ago`;
    }
    if (diffHr > 0) {
      return diffHr === 1 ? '1 hour ago' : `${diffHr} hours ago`;
    }
    if (diffMin > 0) {
      return diffMin === 1 ? '1 minute ago' : `${diffMin} minutes ago`;
    }
    return 'Just now';
  }

  readonly projectsStatusStats = computed(() => {
    let onTrack = 0;
    let atRisk = 0;
    let behind = 0;
    let completed = 0;

    for (const p of this.projects()) {
      const status = this.getProjectStatusLabel(p);
      if (status === 'Completed') completed++;
      else if (status === 'Behind') behind++;
      else if (status === 'At Risk') atRisk++;
      else onTrack++;
    }

    const total = this.projects().length || 1;
    return {
      onTrack,
      atRisk,
      behind,
      completed,
      onTrackPercent: Math.round((onTrack * 100) / total),
      atRiskPercent: Math.round((atRisk * 100) / total),
      behindPercent: Math.round((behind * 100) / total),
      completedPercent: Math.round((completed * 100) / total),
    };
  });

  readonly recentActivities = computed(() => {
    const actMap = this.projectActivitiesMap();
    const allActivities: any[] = [];

    for (const projectId of Object.keys(actMap)) {
      const proj = this.projects().find((p) => p.projectId === projectId);
      if (!proj) continue; // Only show activities for projects the user has access to (the ones in the projects signal)
      
      const list = actMap[projectId] || [];
      for (const act of list) {
        allActivities.push({
          ...act,
          projectName: proj.name,
        });
      }
    }

    allActivities.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    return allActivities.slice(0, 5);
  });

  @ViewChild('fileInput') fileInputRef?: ElementRef<HTMLInputElement>;

  triggerFileInput(): void {
    this.fileInputRef?.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (!target.files || target.files.length === 0) {
      return;
    }
    this.processFile(target.files[0]);
  }

  onFileDropped(event: DragEvent): void {
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.processFile(event.dataTransfer.files[0]);
    }
  }

  private processFile(file: File): void {
    this.loadedFileName.set(file.name);
    const nameWithoutExt = file.name.substring(0, file.name.lastIndexOf('.')) || file.name;
    const formattedName = nameWithoutExt
      .replace(/_|-/g, ' ')
      .replace(/\b\w/g, c => c.toUpperCase());

    this.importForm.patchValue({
      name: formattedName
    });

    const reader = new FileReader();
    if (file.name.endsWith('.json')) {
      reader.onload = (e) => {
        try {
          const json = JSON.parse(e.target?.result as string);
          if (json.name) {
            this.importForm.patchValue({
              name: json.name,
              key: json.key || '',
              projectType: json.projectType || ProjectType.Scrum,
              description: json.description || ''
            });
          }
          const tasks = json.tasks || [];
          this.importedTasks.set(tasks);
        } catch (err) {
          this.error.set('Invalid JSON format. Please upload a valid Workflow.IO project export file.');
          this.clearLoadedFile();
        }
      };
      reader.readAsText(file);
    } else if (file.name.endsWith('.csv')) {
      reader.onload = (e) => {
        try {
          const csvText = e.target?.result as string;
          const tasks = this.parseCSV(csvText);
          this.importedTasks.set(tasks);
        } catch (err) {
          this.error.set('Failed to parse CSV file. Please use the correct headers.');
          this.clearLoadedFile();
        }
      };
      reader.readAsText(file);
    } else if (file.name.endsWith('.xlsx')) {
      reader.onload = (e) => {
        try {
          const arrayBuffer = e.target?.result as ArrayBuffer;
          const tasks = this.parseExcel(arrayBuffer);
          this.importedTasks.set(tasks);
        } catch (err) {
          this.error.set('Failed to parse Excel file. Ensure it contains the "Tasks" sheet and matches the template format.');
          this.clearLoadedFile();
        }
      };
      reader.readAsArrayBuffer(file);
    } else {
      this.error.set('Unsupported file format. Please upload a .json, .csv, or .xlsx file.');
      this.clearLoadedFile();
    }
  }

  private mapPriority(val: string): number {
    const p = val.toLowerCase().trim();
    if (p === 'low' || p === '1') return 1;
    if (p === 'medium' || p === '2') return 2;
    if (p === 'high' || p === '3') return 3;
    if (p === 'critical' || p === '4') return 4;
    return 2; // Default Medium
  }

  private mapIssueType(val: string): number {
    const t = val.toLowerCase().trim();
    if (t === 'story' || t === '1') return 1;
    if (t === 'task' || t === '2') return 2;
    if (t === 'bug' || t === '3') return 3;
    if (t === 'subtask' || t === '4') return 4;
    return 2; // Default Task
  }

  private parseCSV(text: string): ImportTaskDto[] {
    const lines: string[] = [];
    let currentLine = '';
    let inQuotes = false;
    
    for (let i = 0; i < text.length; i++) {
      const char = text[i];
      if (char === '"') {
        inQuotes = !inQuotes;
      } else if (char === '\n' && !inQuotes) {
        lines.push(currentLine.trim());
        currentLine = '';
      } else if (char === '\r') {
        // skip carriage return
      } else {
        currentLine += char;
      }
    }
    if (currentLine) {
      lines.push(currentLine.trim());
    }

    if (lines.length === 0) return [];
    
    const parseCSVRow = (rowText: string): string[] => {
      const row: string[] = [];
      let val = '';
      let quotes = false;
      for (let i = 0; i < rowText.length; i++) {
        const c = rowText[i];
        if (c === '"') {
          quotes = !quotes;
        } else if (c === ',' && !quotes) {
          row.push(val.trim());
          val = '';
        } else {
          val += c;
        }
      }
      row.push(val.trim());
      return row;
    };

    const headers = parseCSVRow(lines[0]);
    const headerIndices: Record<string, number> = {};
    headers.forEach((h, i) => {
      headerIndices[h.trim()] = i;
    });

    const tasks: ImportTaskDto[] = [];
    for (let r = 1; r < lines.length; r++) {
      if (!lines[r]) continue;
      const row = parseCSVRow(lines[r]);
      const getValue = (field: string): string => {
        const idx = headerIndices[field];
        return idx !== undefined && idx < row.length ? row[idx] : '';
      };

      const title = getValue('Title');
      if (!title) continue;

      tasks.push({
        title,
        description: getValue('Description') || undefined,
        priority: this.mapPriority(getValue('Priority')),
        issueType: this.mapIssueType(getValue('IssueType')),
        storyPoints: getValue('StoryPoints') ? parseInt(getValue('StoryPoints'), 10) : undefined,
        dueDate: getValue('DueDate') || undefined
      });
    }
    return tasks;
  }

  private parseExcel(arrayBuffer: ArrayBuffer): ImportTaskDto[] {
    const data = new Uint8Array(arrayBuffer);
    const workbook = read(data, { type: 'array' });
    const sheetName = workbook.SheetNames.includes('Tasks') ? 'Tasks' : workbook.SheetNames[0];
    const worksheet = workbook.Sheets[sheetName];
    const rows = utils.sheet_to_json<any>(worksheet, { raw: false, defval: '' });
    
    const tasks: ImportTaskDto[] = [];
    for (const row of rows) {
      const title = row['Title'];
      if (!title) continue;

      let rawDueDate = row['DueDate'];
      let parsedDate: string | undefined = undefined;
      if (rawDueDate) {
        const d = new Date(rawDueDate);
        if (!isNaN(d.getTime())) {
          parsedDate = d.toISOString().split('T')[0];
        }
      }

      tasks.push({
        title: String(title),
        description: row['Description'] ? String(row['Description']) : undefined,
        priority: this.mapPriority(String(row['Priority'] || '')),
        issueType: this.mapIssueType(String(row['IssueType'] || '')),
        storyPoints: row['StoryPoints'] ? parseInt(row['StoryPoints'], 10) : undefined,
        dueDate: parsedDate
      });
    }
    return tasks;
  }

  confirmImport(): void {
    if (this.importForm.invalid || !this.loadedFileName()) {
      return;
    }
    this.creating.set(true);
    const raw = this.importForm.getRawValue();
    const importPayload = {
      name: raw.name,
      key: raw.key || null,
      projectType: Number(raw.projectType) as ProjectType,
      description: raw.description || null,
      tasks: this.importedTasks()
    };

    this.importProject(importPayload);
  }

  closeImportModal(): void {
    this.showImportModal.set(false);
    this.showSpecs.set(false);
    this.clearLoadedFile();
    this.importForm.reset({ projectType: ProjectType.Scrum });
  }

  clearLoadedFile(): void {
    this.loadedFileName.set(null);
    this.importedTasks.set([]);
    if (this.fileInputRef) {
      this.fileInputRef.nativeElement.value = '';
    }
  }

  private importProject(data: any): void {
    this.loading.set(true);
    this.projectService.importProject(data).subscribe({
      next: (project) => {
        this.creating.set(false);
        this.closeImportModal();
        this.loadProjects();
        void this.router.navigate(['/projects', project.projectId, 'tasks']);
      },
      error: (err) => {
        this.error.set(getApiErrorMessage(err));
        this.creating.set(false);
        this.loading.set(false);
      }
    });
  }

  readonly trends = computed(() => {
    const projectsList = this.projects();
    const tasksMap = this.projectTasksMap();
    const allTasks: TaskResponse[] = [];
    for (const projectId of Object.keys(tasksMap)) {
      allTasks.push(...(tasksMap[projectId] || []));
    }

    const totalProjectsTrend = this.calculateTrend(projectsList);
    const activeProjectsTrend = this.calculateTrend(projectsList.filter(p => {
      const status = this.getProjectStatusLabel(p);
      return status !== 'Completed' && status !== 'Archived';
    }));
    const completedProjectsTrend = this.calculateTrend(projectsList.filter(p => this.getProjectStatusLabel(p) === 'Completed'));
    const totalTasksTrend = this.calculateTrend(allTasks);

    return {
      totalProjects: totalProjectsTrend,
      activeProjects: activeProjectsTrend,
      completedProjects: completedProjectsTrend,
      totalTasks: totalTasksTrend,
    };
  });

  private calculateTrend(items: { createdAt: string }[]): { value: string; isUp: boolean; icon: string; class: string } {
    const now = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(now.getDate() - 30);
    const sixtyDaysAgo = new Date();
    sixtyDaysAgo.setDate(now.getDate() - 60);

    const currentPeriodItems = items.filter(item => {
      const d = new Date(item.createdAt);
      return d >= thirtyDaysAgo && d <= now;
    });

    const previousPeriodItems = items.filter(item => {
      const d = new Date(item.createdAt);
      return d >= sixtyDaysAgo && d < thirtyDaysAgo;
    });

    const currentCount = currentPeriodItems.length;
    const previousCount = previousPeriodItems.length;

    if (previousCount === 0) {
      const val = currentCount > 0 ? '+100%' : '0%';
      return {
        value: val,
        isUp: true,
        icon: 'trending_up',
        class: 'trend-up'
      };
    }

    const diff = currentCount - previousCount;
    const percent = Math.round((diff * 100) / previousCount);
    const prefix = percent >= 0 ? '+' : '';
    
    return {
      value: `${prefix}${percent}%`,
      isUp: percent >= 0,
      icon: percent >= 0 ? 'trending_up' : 'trending_down',
      class: percent >= 0 ? 'trend-up' : 'trend-down'
    };
  }
}
