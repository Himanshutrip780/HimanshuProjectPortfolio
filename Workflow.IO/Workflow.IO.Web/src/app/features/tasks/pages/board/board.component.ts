import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { Component, DestroyRef, effect, inject, OnInit, signal, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

import {
  BoardColumnView,
  BoardViewResponse,
  TaskResponse,
  TaskStatus,
  IssueType,
  TaskPriority,
} from '../../../../core/models/task.models';
import { TeamResponse } from '../../../../core/models/team.models';
import { UserDto } from '../../../../core/models/user.models';
import { RealtimeService } from '../../../../core/services/realtime.service';
import { UserService } from '../../../../core/services/user.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { TaskService } from '../../services/task.service';
import { TeamService } from '../../../teams/services/team.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule, RouterLink, DragDropModule, FormsModule],
  template: `
    <section class="board-container">
      <header class="board-header">
        <div class="header-main" style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:1rem;">
          <div style="display:flex; align-items:center; gap:1rem;">
            <span class="material-symbols-outlined board-header-icon">dashboard</span>
            <div>
              <h1>Kanban Board</h1>
              @if (board(); as view) {
                <p class="subtitle">{{ view.board.name }} · Drag cards to transition status</p>
              }
            </div>
          </div>
          
          <!-- Filters & Sorting Controls -->
          <div style="display:flex; align-items:center; gap:0.75rem; flex-wrap:wrap;">
            <!-- Board Team Filter -->
            <div class="board-team-filter">
              <span class="material-symbols-outlined dropdown-icon">groups</span>
              <select [ngModel]="selectedTeamId()" (ngModelChange)="selectedTeamId.set($event)" class="team-select" title="Filter by Team">
                <option value="">All Teams</option>
                @for (team of teams(); track team.teamId) {
                  <option [value]="team.teamId">{{ team.name }}</option>
                }
              </select>
            </div>

            <!-- Board Sort Dropdown -->
            <div class="board-team-filter">
              <span class="material-symbols-outlined dropdown-icon">sort</span>
              <select [ngModel]="sortBy()" (ngModelChange)="sortBy.set($event)" class="team-select" title="Sort Columns">
                <option value="recent">Sort: Recent</option>
                <option value="priority">Sort: Priority</option>
                <option value="dueDate">Sort: Due Date</option>
                <option value="storyPoints">Sort: Story Points</option>
                <option value="title">Sort: Title (A-Z)</option>
              </select>
            </div>
          </div>
        </div>
      </header>

      <!-- Smart Filter Pills Sub-Header -->
      @if (columns().length > 0 && !loading() && !error()) {
        <div class="board-filters-bar">
          <div class="filter-pills">
            <span class="filter-pills-label">Quick Filters:</span>
            <button 
              type="button" 
              class="filter-pill" 
              [class.active]="activeFilters()['my-tasks']" 
              (click)="toggleFilter('my-tasks')"
            >
              <span class="material-symbols-outlined font-icon">person</span>
              My Tasks
            </button>
            <button 
              type="button" 
              class="filter-pill" 
              [class.active]="activeFilters()['high-priority']" 
              (click)="toggleFilter('high-priority')"
            >
              <span class="material-symbols-outlined font-icon font-red">error</span>
              High Priority
            </button>
            <button 
              type="button" 
              class="filter-pill" 
              [class.active]="activeFilters()['frontend']" 
              (click)="toggleFilter('frontend')"
            >
              <span class="material-symbols-outlined font-icon">code</span>
              Frontend
            </button>
            <button 
              type="button" 
              class="filter-pill" 
              [class.active]="activeFilters()['backend']" 
              (click)="toggleFilter('backend')"
            >
              <span class="material-symbols-outlined font-icon">terminal</span>
              Backend
            </button>
          </div>
          
          @if (hasActiveFilters()) {
            <button type="button" class="btn-clear-filters" (click)="clearFilters()">
              <span class="material-symbols-outlined font-icon">filter_alt_off</span>
              Clear Filters
            </button>
          }
        </div>
      }

      @if (loading()) {
        <div class="board-state">
          <div class="spinner"></div>
          <p>Loading project board...</p>
        </div>
      } @else if (error()) {
        <div class="board-state error-state">
          <span class="material-symbols-outlined">error_outline</span>
          <p>{{ error() }}</p>
        </div>
      } @else if (columns().length === 0) {
        <div class="board-state empty-state">
          <span class="material-symbols-outlined">space_dashboard</span>
          <h3>No Board Configured</h3>
          <p class="subtitle">Get started by creating a default board for this project.</p>
          <button type="button" class="btn btn-primary" (click)="createDefaultBoard()" [disabled]="busy()">
            @if (busy()) {
              <span class="spinner-sm"></span> Creating Board...
            } @else {
              <span class="material-symbols-outlined font-icon">add</span> Create Default Board
            }
          </button>
        </div>
      } @else {
        <div class="board-columns">
          @for (col of filteredColumns(); track col.column.boardColumnId) {
            <div class="board-column-wrapper" [class.collapsed]="isColumnCollapsed(col.column.boardColumnId)" [class]="getColumnColorClass(col.column.status, col.column.name)">
              
              <!-- Column Top Status Stripe -->
              <div class="column-status-bar"></div>

              <!-- Column Header Area -->
              <div class="column-header-container">
                @if (isColumnCollapsed(col.column.boardColumnId)) {
                  <!-- Collapsed Column Stripe Header -->
                  <div class="collapsed-column-header" (click)="toggleColumnCollapse(col.column.boardColumnId)" title="Expand Column">
                    <button type="button" class="btn-col-toggle collapsed-toggle">
                      <span class="material-symbols-outlined">last_page</span>
                    </button>
                    <div class="collapsed-title-wrapper">
                      <h3>{{ col.column.name }}</h3>
                      <span class="column-count">{{ col.tasks.length }}</span>
                    </div>
                  </div>
                } @else {
                  <!-- Normal Open Column Header -->
                  <div class="column-header">
                    <div class="column-title-group">
                      <h3>{{ col.column.name }}</h3>
                      <span class="column-count">{{ col.tasks.length }}</span>
                    </div>
                    <div style="display: flex; align-items: center; gap: 0.25rem;">
                      <button type="button" class="btn-col-toggle" (click)="toggleColumnCollapse(col.column.boardColumnId)" title="Collapse Column">
                        <span class="material-symbols-outlined">first_page</span>
                      </button>
                      <span class="column-actions material-symbols-outlined">more_horiz</span>
                    </div>
                  </div>
                }
              </div>
              
              <!-- Tasks Drop List -->
              <div
                class="board-column"
                [class]="getColumnColorClass(col.column.status, col.column.name) + (isColumnCollapsed(col.column.boardColumnId) ? ' collapsed-column' : '')"
                cdkDropList
                [cdkDropListData]="col.tasks"
                [id]="listId(col)"
                [cdkDropListConnectedTo]="connectedLists()"
                (cdkDropListDropped)="onDrop($event, col)"
                (cdkDropListEntered)="onDragEntered(col)"
                (cdkDropListExited)="onDragExited(col)"
                (click)="isColumnCollapsed(col.column.boardColumnId) && toggleColumnCollapse(col.column.boardColumnId)"
              >
                @if (!isColumnCollapsed(col.column.boardColumnId)) {
                  @for (task of col.tasks; track task.taskId) {
                    <div 
                      [class]="'task-card ' + getColumnColorClass(col.column.status, col.column.name)"
                      cdkDrag 
                      [cdkDragData]="task"
                      (cdkDragEnded)="onDragEnded()"
                    >
                      <div class="task-card-glow" [class]="getIssueTypeClass(task.issueType)"></div>
                      
                      <div class="task-card-header">
                        <span class="task-key">{{ task.issueKey }}</span>
                        @if (task.storyPoints) {
                          <span class="story-points" title="Story Points">{{ task.storyPoints }}</span>
                        }
                      </div>

                      <a [routerLink]="['../../../tasks', task.taskId]" class="task-title">
                        {{ task.title }}
                      </a>

                      <!-- ETA Badge -->
                      @if (task.latestEta) {
                        <div class="eta-badge" [class.delayed]="isDelayed(task)" title="Latest ETA">
                          <span class="material-symbols-outlined eta-icon">schedule</span>
                          <span>ETA: {{ formatDate(task.latestEta) }}</span>
                          @if (isDelayed(task)) { <span class="delayed-label">⚠</span> }
                        </div>
                      }

                      <!-- Dev Update Drawer Toggle -->
                      <div class="drawer-toggle" (click)="toggleDrawer(task.taskId, $event)" cdkDragHandle="false">
                        <span class="drawer-toggle-label">
                          <span class="material-symbols-outlined" style="font-size:0.9rem">edit_note</span>
                          Schedule Info
                        </span>
                        <span class="material-symbols-outlined drawer-chevron" [class.open]="isDrawerOpen(task.taskId)">
                          expand_more
                        </span>
                      </div>

                      <!-- Collapsible Drawer -->
                      @if (isDrawerOpen(task.taskId)) {
                        <div class="dev-drawer" (mousedown)="$event.stopPropagation()">
                          <div class="drawer-row">
                            <div class="drawer-field">
                              <label class="drawer-label">Initial ETA</label>
                              <input
                                type="date"
                                class="drawer-input"
                                [ngModel]="getDrawerDraft(task.taskId, 'initialEta')"
                                (ngModelChange)="onDraftChange(task.taskId, 'initialEta', $event)"
                              />
                            </div>
                            <div class="drawer-field">
                              <label class="drawer-label">Latest ETA</label>
                              <input
                                type="date"
                                class="drawer-input"
                                [ngModel]="getDrawerDraft(task.taskId, 'latestEta')"
                                (ngModelChange)="onDraftChange(task.taskId, 'latestEta', $event)"
                              />
                            </div>
                          </div>
                          <div class="drawer-actions">
                            <button
                              type="button"
                              class="drawer-save-btn"
                              (click)="saveDevInfo(task, $event)"
                              [disabled]="savingDrawer() === task.taskId"
                            >
                              @if (savingDrawer() === task.taskId) {
                                Saving…
                              } @else {
                                Save
                              }
                            </button>
                            @if (savedDrawer() === task.taskId) {
                              <span class="saved-indicator">✓ Saved</span>
                            }
                          </div>
                        </div>
                      }

                      <div class="task-card-footer">
                        <div class="task-tags">
                          <div class="badge-pill type-pill" [class]="getIssueTypeClass(task.issueType)" [title]="getIssueTypeLabel(task.issueType)">
                            <span class="material-symbols-outlined badge-icon">
                              {{ getIssueTypeIcon(task.issueType) }}
                            </span>
                            <span class="badge-text">{{ getIssueTypeLabel(task.issueType) }}</span>
                          </div>
                          
                          <div class="badge-pill priority-pill" [class]="getPriorityClass(task.priority)" [title]="getPriorityLabel(task.priority)">
                            <span class="material-symbols-outlined badge-icon">
                              {{ getPriorityIcon(task.priority) }}
                            </span>
                          </div>

                          @if (task.isOverdue) {
                            <div class="badge-pill overdue-pill" title="Overdue">
                              <span class="material-symbols-outlined badge-icon text-rose-500">warning</span>
                            </div>
                          }
                        </div>

                        <div class="task-assignee">
                          @if (getUserAvatarUrl(task.assigneeId); as avatar) {
                            <img class="assignee-avatar" [src]="avatar" [title]="getUserDisplayName(task.assigneeId)" alt="Avatar" />
                          } @else {
                            <div class="assignee-avatar" [style.background-color]="getAssigneeColor(task.assigneeId)" [title]="getUserDisplayName(task.assigneeId)">
                              {{ getUserInitials(task.assigneeId) }}
                            </div>
                          }
                        </div>
                      </div>

                      <!-- Premium Custom Drag Preview with Adaptive Neon Glow -->
                      <div *cdkDragPreview [class]="'task-card cdk-drag-preview ' + (hoveredColumnClass() || getColumnColorClass(col.column.status, col.column.name))">
                        <div class="task-card-header">
                          <span class="task-key">{{ task.issueKey }}</span>
                          @if (task.storyPoints) {
                            <span class="story-points">{{ task.storyPoints }}</span>
                          }
                        </div>
                        <div class="task-title" style="margin: 0.25rem 0; font-weight:600; font-size: 0.9rem;">{{ task.title }}</div>
                        <div class="task-card-footer" style="border-top:none; margin-top:0.25rem;">
                          <div class="badge-pill type-pill" [class]="getIssueTypeClass(task.issueType)">
                            <span class="material-symbols-outlined badge-icon">
                              {{ getIssueTypeIcon(task.issueType) }}
                            </span>
                            <span class="badge-text">{{ getIssueTypeLabel(task.issueType) }}</span>
                          </div>
                          <div class="task-assignee">
                            @if (getUserAvatarUrl(task.assigneeId); as avatar) {
                              <img class="assignee-avatar" [src]="avatar" style="width:20px; height:20px;" alt="Avatar" />
                            } @else {
                              <div class="assignee-avatar" [style.background-color]="getAssigneeColor(task.assigneeId)" style="width:20px; height:20px; font-size:0.6rem;">
                                  {{ getUserInitials(task.assigneeId) }}
                              </div>
                            }
                          </div>
                        </div>
                      </div>

                      <!-- Animated Ghost Dropzone Placeholder -->
                      <div *cdkDragPlaceholder [class]="'ghost-dropzone ' + getColumnColorClass(col.column.status, col.column.name)">
                        <div class="ghost-dropzone-inner">
                          <span class="material-symbols-outlined">move_item</span>
                          <span>Drop here</span>
                        </div>
                      </div>
                    </div>
                  } @empty {
                    <div class="column-empty-state">
                      <span class="material-symbols-outlined">inbox</span>
                      <span>No tasks here</span>
                    </div>
                  }
                }
              </div>
            </div>
          }
        </div>
      }
    </section>
  `,
  styles: `
    .board-container {
      padding: 2.5rem;
      display: flex;
      flex-direction: column;
      height: 100%;
      min-height: calc(100vh - 120px);
      gap: 2rem;
      position: relative;
      background: radial-gradient(circle at 5% 10%, rgba(99, 102, 241, 0.07) 0%, transparent 35%),
                  radial-gradient(circle at 95% 85%, rgba(139, 92, 246, 0.05) 0%, transparent 45%),
                  var(--bg-body);
      transition: background var(--transition-normal);
    }

    .board-header {
      margin-bottom: 0.5rem;
    }

    .board-header h1 {
      font-size: 2.25rem;
      font-weight: 850;
      letter-spacing: -0.04em;
      background: linear-gradient(135deg, var(--text-primary) 0%, #8b5cf6 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      margin: 0;
    }

    .board-header-icon {
      font-size: 2rem;
      color: #ffffff;
      background: var(--primary-gradient);
      padding: 0.6rem;
      border-radius: 14px;
      display: flex;
      align-items: center;
      justify-content: center;
      border: 1px solid rgba(255, 255, 255, 0.15);
      box-shadow: 0 8px 24px rgba(99, 102, 241, 0.25);
      transition: all var(--transition-normal);
    }
    .board-header-icon:hover {
      transform: rotate(-3deg) scale(1.05);
      box-shadow: 0 10px 28px rgba(99, 102, 241, 0.35);
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 0.9rem;
      margin-top: 0.35rem;
      font-weight: 500;
      opacity: 0.85;
    }

    /* Board Team Filter Select */
    .board-team-filter {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background: rgba(255, 255, 255, 0.7);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      padding: 0.5rem 0.95rem;
      transition: all var(--transition-normal);
      box-shadow: 0 4px 10px rgba(0, 0, 0, 0.015);
    }
    .dark-theme .board-team-filter,
    [data-theme='dark'] .board-team-filter {
      background: rgba(30, 41, 59, 0.5);
      border-color: rgba(255, 255, 255, 0.05);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
    }
    .board-team-filter:hover {
      border-color: var(--primary-color);
      transform: translateY(-1.5px);
      box-shadow: 0 6px 16px rgba(99, 102, 241, 0.08);
    }
    .board-team-filter:focus-within {
      border-color: var(--primary-color);
      box-shadow: 0 0 0 3px var(--primary-glow);
    }
    .board-team-filter .dropdown-icon {
      color: var(--primary-color);
      font-size: 1.35rem;
    }
    .board-team-filter .team-select {
      background: transparent;
      border: none;
      outline: none;
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-primary);
      cursor: pointer;
      padding: 0;
      width: auto;
      min-width: 140px;
    }
    .board-team-filter .team-select:focus {
      box-shadow: none;
    }

    /* Smart Filters sub-header bar */
    .board-filters-bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.85rem 1.5rem;
      background: rgba(255, 255, 255, 0.45);
      backdrop-filter: blur(24px) saturate(120%);
      -webkit-backdrop-filter: blur(24px) saturate(120%);
      border: 1px solid rgba(255, 255, 255, 0.35);
      border-radius: 16px;
      gap: 1rem;
      flex-wrap: wrap;
      box-shadow: 0 12px 34px rgba(0, 0, 0, 0.03);
      transition: all var(--transition-normal);
    }
    .dark-theme .board-filters-bar,
    [data-theme='dark'] .board-filters-bar {
      background: rgba(26, 34, 54, 0.65);
      border-color: rgba(255, 255, 255, 0.06);
      box-shadow: 0 12px 34px rgba(0, 0, 0, 0.25);
    }

    .filter-pills {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .filter-pills-label {
      font-size: 0.75rem;
      font-weight: 800;
      color: var(--text-muted);
      margin-right: 0.5rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }

    .filter-pill {
      display: inline-flex;
      align-items: center;
      gap: 0.45rem;
      padding: 0.5rem 1.15rem;
      font-size: 0.8rem;
      font-weight: 600;
      border-radius: 99px;
      border: 1px solid var(--border-color);
      background: var(--bg-card);
      color: var(--text-secondary);
      cursor: pointer;
      transition: all var(--transition-normal) var(--motion-easing);
      user-select: none;
    }
    .dark-theme .filter-pill,
    [data-theme='dark'] .filter-pill {
      background: rgba(30, 41, 59, 0.6);
      border-color: rgba(255, 255, 255, 0.05);
    }

    .filter-pill:hover {
      background: var(--bg-hover);
      color: var(--text-primary);
      border-color: var(--primary-color);
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(99, 102, 241, 0.1);
    }

    .filter-pill.active {
      background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
      color: #ffffff;
      border-color: transparent;
      box-shadow: 0 6px 16px rgba(99, 102, 241, 0.4);
      transform: translateY(-2px);
    }
    .filter-pill.active:hover {
      box-shadow: 0 8px 22px rgba(99, 102, 241, 0.5);
    }

    .btn-clear-filters {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      font-size: 0.775rem;
      font-weight: 700;
      color: var(--danger-color);
      background: transparent;
      border: none;
      cursor: pointer;
      padding: 0.4rem 0.85rem;
      border-radius: 10px;
      transition: all var(--transition-normal);
    }
    .btn-clear-filters:hover {
      background: rgba(239, 68, 68, 0.08);
      transform: translateY(-1px);
    }

    .board-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 6rem 2rem;
      background: rgba(255, 255, 255, 0.2);
      backdrop-filter: blur(10px);
      border: 2px dashed var(--border-color);
      border-radius: var(--radius-xl);
      text-align: center;
      color: var(--text-secondary);
      gap: 1.25rem;
      box-shadow: var(--shadow-sm);
    }
    .dark-theme .board-state,
    [data-theme='dark'] .board-state {
      background: rgba(15, 23, 42, 0.15);
      border-color: rgba(255, 255, 255, 0.08);
    }
    .board-state h3 {
      font-size: 1.25rem;
      font-weight: 800;
      color: var(--text-primary);
      margin: 0;
    }
    .board-state span {
      font-size: 3.5rem;
      color: var(--text-muted);
    }

    .board-columns {
      display: flex;
      gap: 1.5rem;
      overflow-x: auto;
      align-items: flex-start;
      padding-bottom: 2rem;
      min-height: 600px;
    }

    /* Column Wrapper and Status Colors */
    .board-column-wrapper {
      flex: 1;
      min-width: 300px;
      max-width: 380px;
      display: flex;
      flex-direction: column;
      gap: 0.85rem;
      background: rgba(255, 255, 255, 0.5);
      backdrop-filter: blur(16px);
      -webkit-backdrop-filter: blur(16px);
      border: 1px solid rgba(255, 255, 255, 0.45);
      border-radius: 20px;
      overflow: hidden;
      box-shadow: 0 10px 30px -10px rgba(0, 0, 0, 0.02);
      transition: all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
    }
    .dark-theme .board-column-wrapper,
    [data-theme='dark'] .board-column-wrapper {
      background: rgba(15, 23, 42, 0.35);
      border-color: rgba(255, 255, 255, 0.04);
      box-shadow: 0 10px 30px -10px rgba(0, 0, 0, 0.22);
    }

    /* Column Top Status Stripe */
    .column-status-bar {
      height: 5px;
      width: 100%;
      transition: all var(--transition-normal);
    }
    .status-todo .column-status-bar { background: linear-gradient(90deg, #64748b 0%, #94a3b8 100%); }
    .status-progress .column-status-bar { background: linear-gradient(90deg, #d97706 0%, #f59e0b 100%); }
    .status-review .column-status-bar { background: linear-gradient(90deg, #7c3aed 0%, #8b5cf6 100%); }
    .status-done .column-status-bar { background: linear-gradient(90deg, #059669 0%, #10b981 100%); }
    .status-blocked .column-status-bar { background: linear-gradient(90deg, #dc2626 0%, #ef4444 100%); }

    .board-column-wrapper.collapsed {
      min-width: 64px !important;
      max-width: 64px !important;
      flex: 0 0 64px !important;
      cursor: pointer;
      background: rgba(255, 255, 255, 0.35);
    }
    .dark-theme .board-column-wrapper.collapsed,
    [data-theme='dark'] .board-column-wrapper.collapsed {
      background: rgba(15, 23, 42, 0.2);
    }

    .collapsed-column-header {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1.5rem;
      padding: 1.25rem 0;
      user-select: none;
    }

    .collapsed-title-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.85rem;
      writing-mode: vertical-rl;
      text-orientation: mixed;
      color: var(--text-secondary);
    }

    .collapsed-title-wrapper h3 {
      font-size: 0.8rem;
      font-weight: 800;
      letter-spacing: 0.12em;
      text-transform: uppercase;
      margin: 0;
      white-space: nowrap;
    }

    .collapsed-title-wrapper .column-count {
      writing-mode: horizontal-tb;
      margin-top: 0.35rem;
      background: var(--border-color);
      color: var(--text-primary);
      font-size: 0.725rem;
      font-weight: 800;
      padding: 0.2rem 0.5rem;
      border-radius: 99px;
      line-height: 1;
    }

    .btn-col-toggle {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      padding: 0.3rem;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all var(--transition-normal);
    }
    
    .btn-col-toggle span {
      transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      font-size: 1.35rem;
    }
    
    .btn-col-toggle:hover {
      color: var(--primary-color);
      background: var(--bg-hover);
    }
    
    .btn-col-toggle:hover span {
      transform: scale(1.2);
    }
    
    .column-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.25rem 0.5rem;
    }

    .column-title-group {
      display: flex;
      align-items: center;
      gap: 0.65rem;
    }

    .column-header h3 {
      font-size: 0.85rem;
      font-weight: 850;
      color: var(--text-primary);
      text-transform: uppercase;
      letter-spacing: 0.08em;
      margin: 0;
    }

    .column-count {
      background: rgba(99, 102, 241, 0.08);
      border: 1px solid rgba(99, 102, 241, 0.12);
      color: var(--primary-color);
      font-size: 0.725rem;
      font-weight: 800;
      padding: 0.15rem 0.55rem;
      border-radius: 99px;
      line-height: 1;
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }
    .dark-theme .column-count,
    [data-theme='dark'] .column-count {
      background: rgba(99, 102, 241, 0.15);
      border-color: rgba(99, 102, 241, 0.25);
      color: #a5b4fc;
    }

    .column-actions {
      color: var(--text-muted);
      cursor: pointer;
      font-size: 1.25rem;
      transition: color var(--transition-fast);
    }
    .column-actions:hover {
      color: var(--text-primary);
    }

    .board-column {
      background: transparent;
      padding: 1rem;
      min-height: 500px;
      display: flex;
      flex-direction: column;
      gap: 0.85rem;
      transition: all 0.3s ease;
    }

    .board-column.collapsed-column {
      min-height: 500px;
      flex: 1;
      width: 100%;
      background: transparent;
      padding: 0;
    }

    /* Column active highlights on drag over */
    .board-column.cdk-drop-list-dragging {
      background: rgba(99, 102, 241, 0.015);
    }

    /* Premium Glassmorphic Task Card */
    .task-card {
      position: relative;
      background: rgba(255, 255, 255, 0.85);
      backdrop-filter: blur(10px);
      -webkit-backdrop-filter: blur(10px);
      border: 1px solid rgba(255, 255, 255, 0.5);
      border-radius: 14px;
      padding: 1.1rem 1.15rem 1.1rem 1.45rem;
      display: flex;
      flex-direction: column;
      gap: 0.8rem;
      cursor: grab;
      box-shadow: 0 4px 10px rgba(0, 0, 0, 0.01);
      overflow: hidden;
      transition: all var(--transition-normal) var(--motion-easing);
    }
    .dark-theme .task-card,
    [data-theme='dark'] .task-card {
      border-color: rgba(255, 255, 255, 0.04);
      background: rgba(26, 34, 54, 0.7);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    }
    
    .task-card:active {
      cursor: grabbing;
    }

    .task-card.fade-out {
      opacity: 0.25;
      filter: grayscale(40%);
      pointer-events: none;
    }

    /* Neon glow hover shadows per status color */
    .task-card.status-todo:hover {
      border-color: rgba(148, 163, 184, 0.45);
      box-shadow: 0 12px 24px -10px rgba(148, 163, 184, 0.25), 0 4px 10px rgba(0, 0, 0, 0.05);
      transform: translateY(-4px) scale(1.015);
    }
    .task-card.status-progress:hover {
      border-color: rgba(245, 158, 11, 0.55);
      box-shadow: 0 12px 24px -10px rgba(245, 158, 11, 0.35), 0 4px 10px rgba(0, 0, 0, 0.05);
      transform: translateY(-4px) scale(1.015);
    }
    .task-card.status-review:hover {
      border-color: rgba(139, 92, 246, 0.55);
      box-shadow: 0 12px 24px -10px rgba(139, 92, 246, 0.4), 0 4px 10px rgba(0, 0, 0, 0.05);
      transform: translateY(-4px) scale(1.015);
    }
    .task-card.status-done:hover {
      border-color: rgba(16, 185, 129, 0.55);
      box-shadow: 0 12px 24px -10px rgba(16, 185, 129, 0.35), 0 4px 10px rgba(0, 0, 0, 0.05);
      transform: translateY(-4px) scale(1.015);
    }
    .task-card.status-blocked:hover {
      border-color: rgba(239, 68, 68, 0.55);
      box-shadow: 0 12px 24px -10px rgba(239, 68, 68, 0.4), 0 4px 10px rgba(0, 0, 0, 0.05);
      transform: translateY(-4px) scale(1.015);
    }

    .task-card-glow {
      position: absolute;
      top: 0;
      left: 0;
      bottom: 0;
      width: 4px;
      border-radius: 4px 0 0 4px;
    }
    .task-card-glow.type-story { background-color: var(--success-color); box-shadow: 0 0 8px var(--success-color); }
    .task-card-glow.type-task { background-color: var(--primary-color); box-shadow: 0 0 8px var(--primary-color); }
    .task-card-glow.type-bug { background-color: var(--danger-color); box-shadow: 0 0 8px var(--danger-color); }
    .task-card-glow.type-subtask { background-color: var(--info-color); box-shadow: 0 0 8px var(--info-color); }

    .task-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .task-key {
      font-size: 0.725rem;
      font-weight: 800;
      color: var(--text-muted);
      background: rgba(0, 0, 0, 0.04);
      border: 1px solid rgba(0, 0, 0, 0.03);
      padding: 0.15rem 0.5rem;
      border-radius: 6px;
      letter-spacing: 0.03em;
    }
    .dark-theme .task-key,
    [data-theme='dark'] .task-key {
      background: rgba(255, 255, 255, 0.05);
      border-color: rgba(255, 255, 255, 0.05);
      color: var(--text-secondary);
    }

    .story-points {
      background: var(--primary-glow);
      color: var(--primary-color);
      font-size: 0.7rem;
      font-weight: 800;
      padding: 0.15rem 0.55rem;
      border-radius: var(--radius-sm);
    }

    .task-title {
      font-size: 0.9rem;
      font-weight: 650;
      color: var(--text-primary);
      text-decoration: none;
      line-height: 1.45;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      text-overflow: ellipsis;
      transition: color var(--transition-fast);
    }
    .task-title:hover {
      color: var(--primary-color);
    }

    .task-card-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 0.25rem;
    }

    .task-tags {
      display: flex;
      align-items: center;
      gap: 0.45rem;
    }

    /* Badge Pill Container Style */
    .badge-pill {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border-radius: 8px;
      padding: 0.22rem 0.55rem;
      font-size: 0.675rem;
      font-weight: 750;
      gap: 0.25rem;
      background: var(--bg-hover);
      color: var(--text-secondary);
      border: 1px solid var(--border-color);
      letter-spacing: 0.02em;
    }
    .badge-pill .badge-icon {
      font-size: 0.95rem;
    }

    /* Custom styled badges */
    .badge-pill.type-pill.type-story { background: rgba(16, 185, 129, 0.06); color: #10b981; border-color: rgba(16, 185, 129, 0.12); }
    .badge-pill.type-pill.type-task { background: rgba(99, 102, 241, 0.06); color: #6366f1; border-color: rgba(99, 102, 241, 0.12); }
    .badge-pill.type-pill.type-bug { background: rgba(239, 68, 68, 0.06); color: #ef4444; border-color: rgba(239, 68, 68, 0.12); }
    .badge-pill.type-pill.type-subtask { background: rgba(59, 130, 246, 0.06); color: #3b82f6; border-color: rgba(59, 130, 246, 0.12); }

    .dark-theme .badge-pill.type-pill.type-story, [data-theme='dark'] .badge-pill.type-pill.type-story { background: rgba(52, 211, 153, 0.1); color: #34d399; border-color: rgba(52, 211, 153, 0.15); }
    .dark-theme .badge-pill.type-pill.type-task, [data-theme='dark'] .badge-pill.type-pill.type-task { background: rgba(129, 140, 248, 0.1); color: #818cf8; border-color: rgba(129, 140, 248, 0.15); }
    .dark-theme .badge-pill.type-pill.type-bug, [data-theme='dark'] .badge-pill.type-pill.type-bug { background: rgba(248, 113, 113, 0.1); color: #f87171; border-color: rgba(248, 113, 113, 0.15); }
    .dark-theme .badge-pill.type-pill.type-subtask, [data-theme='dark'] .badge-pill.type-pill.type-subtask { background: rgba(96, 165, 250, 0.1); color: #60a5fa; border-color: rgba(96, 165, 250, 0.15); }

    .badge-pill.priority-pill.priority-low { color: var(--text-muted); background: rgba(148, 163, 184, 0.05); border-color: rgba(148, 163, 184, 0.12); }
    .badge-pill.priority-pill.priority-medium { color: #d97706; background: rgba(245, 158, 11, 0.05); border-color: rgba(245, 158, 11, 0.12); }
    .badge-pill.priority-pill.priority-high { color: #dc2626; background: rgba(239, 68, 68, 0.05); border-color: rgba(239, 68, 68, 0.12); }
    .badge-pill.priority-pill.priority-critical { color: #ef4444; background: rgba(239, 68, 68, 0.12); border-color: rgba(239, 68, 68, 0.25); font-weight: 850; animation: pulse 2s infinite; }

    .dark-theme .badge-pill.priority-pill.priority-medium, [data-theme='dark'] .badge-pill.priority-pill.priority-medium { color: #fbbf24; background: rgba(251, 191, 36, 0.08); border-color: rgba(251, 191, 36, 0.15); }
    .dark-theme .badge-pill.priority-pill.priority-high, [data-theme='dark'] .badge-pill.priority-pill.priority-high { color: #f87171; background: rgba(248, 113, 113, 0.08); border-color: rgba(248, 113, 113, 0.15); }

    .badge-pill.overdue-pill {
      background: rgba(239, 68, 68, 0.06);
      border-color: rgba(239, 68, 68, 0.12);
      color: #ef4444;
    }

    .assignee-avatar {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 0.675rem;
      font-weight: 800;
      border: 1.5px solid var(--bg-card);
      box-shadow: 0 0 0 1.5px var(--border-color);
      object-fit: cover;
      transition: transform var(--transition-fast) var(--motion-easing);
    }
    .assignee-avatar:hover {
      transform: scale(1.15);
      z-index: 2;
    }

    .column-empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 0.65rem;
      padding: 3.5rem 1.25rem;
      color: var(--text-muted);
      font-size: 0.85rem;
      border: 1.5px dashed var(--border-color);
      border-radius: 14px;
      background: rgba(0, 0, 0, 0.015);
      transition: all var(--transition-normal);
    }
    .dark-theme .column-empty-state,
    [data-theme='dark'] .column-empty-state {
      background: rgba(255, 255, 255, 0.015);
      border-color: rgba(255, 255, 255, 0.05);
    }
    .column-empty-state span:first-child {
      font-size: 1.85rem;
      color: var(--text-muted);
      transition: transform var(--transition-normal);
    }
    .column-empty-state:hover span:first-child {
      transform: translateY(-2px);
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3.5px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .spinner-sm {
      width: 14px;
      height: 14px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      display: inline-block;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* ETA badge */
    .eta-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.725rem;
      font-weight: 700;
      color: var(--text-muted);
    }
    .eta-badge.delayed {
      color: var(--danger-color);
    }
    .eta-icon {
      font-size: 0.9rem;
    }
    .delayed-label {
      font-weight: 800;
    }

    /* Drawer toggle */
    .drawer-toggle {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.4rem 0.75rem;
      cursor: pointer;
      border: 1px solid var(--border-color);
      border-radius: 10px;
      margin-top: 0.25rem;
      color: var(--text-muted);
      font-size: 0.75rem;
      background: rgba(255, 255, 255, 0.03);
      transition: all var(--transition-normal);
      user-select: none;
    }
    .drawer-toggle:hover {
      color: var(--primary-color);
      border-color: var(--primary-color);
      background: var(--bg-hover);
      transform: translateY(-0.5px);
    }
    .drawer-toggle-label {
      display: flex;
      align-items: center;
      gap: 0.35rem;
      font-weight: 750;
    }
    .drawer-chevron {
      font-size: 1.2rem;
      transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    }
    .drawer-chevron.open {
      transform: rotate(180deg);
    }

    /* Collapsible dev drawer */
    .dev-drawer {
      display: flex;
      flex-direction: column;
      gap: 0.65rem;
      padding: 0.85rem;
      background: rgba(15, 23, 42, 0.55);
      backdrop-filter: blur(8px);
      -webkit-backdrop-filter: blur(8px);
      border-radius: 10px;
      border: 1px solid var(--border-color);
      margin-top: 0.35rem;
      animation: slideDown 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    }
    @keyframes slideDown {
      from { opacity: 0; transform: translateY(-8px); }
      to   { opacity: 1; transform: translateY(0); }
    }

    .drawer-row {
      display: flex;
      gap: 0.65rem;
      width: 100%;
    }

    .drawer-field {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      flex: 1;
    }

    .drawer-label {
      font-size: 0.65rem;
      font-weight: 850;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }

    .drawer-input {
      width: 100%;
      font-size: 0.775rem;
      padding: 0.35rem 0.55rem;
      border: 1.5px solid var(--border-color);
      border-radius: 6px;
      background: var(--bg-input);
      color: var(--text-primary);
      transition: border-color var(--transition-fast);
      box-sizing: border-box;
    }
    .drawer-input:focus {
      outline: none;
      border-color: var(--primary-color);
    }

    .drawer-actions {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      margin-top: 0.25rem;
    }

    .drawer-save-btn {
      padding: 0.35rem 0.95rem;
      font-size: 0.75rem;
      font-weight: 750;
      border: none;
      border-radius: 6px;
      background: var(--primary-gradient);
      color: #fff;
      cursor: pointer;
      transition: opacity var(--transition-fast);
    }
    .drawer-save-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .drawer-save-btn:hover:not(:disabled) {
      opacity: 0.9;
    }

    .saved-indicator {
      font-size: 0.75rem;
      color: var(--success-color);
      font-weight: 800;
      animation: fadeIn 0.3s ease;
    }

    /* Premium Custom Drag Preview and Dropzone Placeholder */
    .cdk-drag-preview {
      box-sizing: border-box;
      border-radius: 14px;
      opacity: 0.95;
      backdrop-filter: blur(16px) !important;
      -webkit-backdrop-filter: blur(16px) !important;
      z-index: 1000 !important;
      transform: rotate(2.5deg) scale(1.02) !important;
      box-shadow: 0 20px 35px -15px rgba(0, 0, 0, 0.3) !important;
      transition: transform 0.1s ease !important;
    }
    
    .cdk-drag-preview.status-todo {
      box-shadow: 0 15px 30px rgba(100, 116, 139, 0.2), 0 10px 25px rgba(0, 0, 0, 0.12) !important;
      border: 1px solid rgba(100, 116, 139, 0.6) !important;
      background: var(--bg-card) !important;
    }
    .cdk-drag-preview.status-progress {
      box-shadow: 0 15px 30px rgba(245, 158, 11, 0.25), 0 10px 25px rgba(0, 0, 0, 0.12) !important;
      border: 1px solid rgba(245, 158, 11, 0.6) !important;
      background: var(--bg-card) !important;
    }
    .cdk-drag-preview.status-review {
      box-shadow: 0 15px 30px rgba(139, 92, 246, 0.3), 0 10px 25px rgba(0, 0, 0, 0.12) !important;
      border: 1px solid rgba(139, 92, 246, 0.6) !important;
      background: var(--bg-card) !important;
    }
    .cdk-drag-preview.status-done {
      box-shadow: 0 15px 30px rgba(16, 185, 129, 0.25), 0 10px 25px rgba(0, 0, 0, 0.12) !important;
      border: 1px solid rgba(16, 185, 129, 0.6) !important;
      background: var(--bg-card) !important;
    }
    .cdk-drag-preview.status-blocked {
      box-shadow: 0 15px 30px rgba(239, 68, 68, 0.3), 0 10px 25px rgba(0, 0, 0, 0.12) !important;
      border: 1px solid rgba(239, 68, 68, 0.6) !important;
      background: var(--bg-card) !important;
    }

    /* Animated Ghost Dropzone Placeholder */
    .ghost-dropzone {
      border: 2.5px dashed var(--border-color) !important;
      border-radius: 14px;
      min-height: 84px;
      background: rgba(99, 102, 241, 0.03) !important;
      margin-bottom: 0.5rem;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all var(--transition-normal) var(--motion-easing);
      animation: expandHeight 0.2s cubic-bezier(0.4, 0, 0.2, 1) forwards, pulseGhost 2s infinite ease-in-out;
      box-sizing: border-box;
    }
    
    .ghost-dropzone.status-todo { border-color: rgba(100, 116, 139, 0.45) !important; }
    .ghost-dropzone.status-progress { border-color: rgba(245, 158, 11, 0.45) !important; }
    .ghost-dropzone.status-review { border-color: rgba(139, 92, 246, 0.45) !important; }
    .ghost-dropzone.status-done { border-color: rgba(16, 185, 129, 0.45) !important; }
    .ghost-dropzone.status-blocked { border-color: rgba(239, 68, 68, 0.45) !important; }

    .ghost-dropzone-inner {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-weight: 800;
      font-size: 0.8rem;
      color: var(--text-muted);
    }
    
    .ghost-dropzone.status-todo .ghost-dropzone-inner { color: #64748b; }
    .ghost-dropzone.status-progress .ghost-dropzone-inner { color: #f59e0b; }
    .ghost-dropzone.status-review .ghost-dropzone-inner { color: #8b5cf6; }
    .ghost-dropzone.status-done .ghost-dropzone-inner { color: #10b981; }
    .ghost-dropzone.status-blocked .ghost-dropzone-inner { color: #ef4444; }

    @keyframes expandHeight {
      from { min-height: 0; height: 0; opacity: 0; }
      to { min-height: 84px; height: auto; opacity: 1; }
    }
    
    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.55; }
    }
    
    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes pulseGhost {
      0%, 100% { opacity: 0.6; transform: scale(0.99); }
      50% { opacity: 1; transform: scale(1); }
    }
  `,
})
export class BoardComponent implements OnInit {
  readonly IssueType = IssueType;
  readonly TaskPriority = TaskPriority;
  readonly TaskStatus = TaskStatus;

  private readonly taskService = inject(TaskService);
  private readonly teamService = inject(TeamService);
  private readonly userService = inject(UserService);
  private readonly authService = inject(AuthService);
  private readonly realtime = inject(RealtimeService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly board = signal<BoardViewResponse | null>(null);
  readonly columns = signal<BoardColumnView[]>([]);
  readonly teams = signal<TeamResponse[]>([]);
  readonly usersMap = this.userService.usersMap;
  readonly selectedTeamId = signal<string>('');
  readonly sortBy = signal<string>('recent');

  // Premium Features States
  readonly collapsedColumnIds = signal<Record<string, boolean>>({});
  readonly activeFilters = signal<Record<string, boolean>>({});
  readonly hoveredColumnClass = signal<string | null>(null);

  readonly filteredColumns = computed(() => {
    const teamId = this.selectedTeamId();
    const sortBy = this.sortBy();
    const cols = this.columns();

    return cols.map((col) => {
      // 1. Filter tasks by team and quick filter pills
      let tasks = col.tasks.filter((t) => {
        // Team filter
        if (teamId && t.teamId?.toLowerCase() !== teamId.toLowerCase()) {
          return false;
        }
        // Quick filter pills
        if (this.isCardFilteredOut(t)) {
          return false;
        }
        return true;
      });

      // 2. Sort tasks inside this column
      if (sortBy === 'recent') {
        tasks.sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime());
      } else if (sortBy === 'priority') {
        tasks.sort((a, b) => (b.priority || 0) - (a.priority || 0));
      } else if (sortBy === 'dueDate') {
        tasks.sort((a, b) => {
          if (!a.dueDate) return 1;
          if (!b.dueDate) return -1;
          return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
        });
      } else if (sortBy === 'storyPoints') {
        tasks.sort((a, b) => (b.storyPoints || 0) - (a.storyPoints || 0));
      } else if (sortBy === 'title') {
        tasks.sort((a, b) => a.title.localeCompare(b.title));
      }

      return {
        ...col,
        tasks
      };
    });
  });

  readonly hasActiveFilters = computed(() => 
    Object.values(this.activeFilters()).some(Boolean)
  );

  readonly loading = signal(true);
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly savingDrawer = signal<string | null>(null);
  readonly savedDrawer = signal<string | null>(null);
  private statusChangeInFlight = false;

  // Tracks which task cards have the drawer open
  private openDrawers = new Set<string>();
  // Stores draft edits keyed by taskId:field
  private drawerDrafts = new Map<string, string>();

  // Collapsible Columns Helpers
  toggleColumnCollapse(colId: string): void {
    this.collapsedColumnIds.update(ids => ({
      ...ids,
      [colId]: !ids[colId]
    }));
  }

  isColumnCollapsed(colId: string): boolean {
    return !!this.collapsedColumnIds()[colId];
  }

  // Smart Filters Helpers
  toggleFilter(filterKey: string): void {
    this.activeFilters.update(filters => ({
      ...filters,
      [filterKey]: !filters[filterKey]
    }));
  }

  clearFilters(): void {
    this.activeFilters.set({});
  }

  // Drag Hover Helpers
  onDragEntered(col: BoardColumnView): void {
    const cls = this.getColumnColorClass(col.column.status, col.column.name);
    this.hoveredColumnClass.set(cls);
  }

  onDragExited(col: BoardColumnView): void {
    const cls = this.getColumnColorClass(col.column.status, col.column.name);
    if (this.hoveredColumnClass() === cls) {
      this.hoveredColumnClass.set(null);
    }
  }

  onDragEnded(): void {
    this.hoveredColumnClass.set(null);
  }

  isCardFilteredOut(task: TaskResponse): boolean {
    const filters = this.activeFilters();
    const activeKeys = Object.keys(filters).filter(k => filters[k]);
    if (activeKeys.length === 0) return false;

    const currentUserId = this.authService.currentUser()?.userId;

    for (const key of activeKeys) {
      if (key === 'my-tasks') {
        if (task.assigneeId?.toLowerCase() !== currentUserId?.toLowerCase()) return true;
      }
      if (key === 'high-priority') {
        if (task.priority !== TaskPriority.High && task.priority !== TaskPriority.Critical) return true;
      }
      if (key === 'frontend') {
        const hasFeDev = !!task.feDeveloper;
        const hasFeTitle = task.title.toLowerCase().includes('fe') || task.title.toLowerCase().includes('frontend');
        if (!hasFeDev && !hasFeTitle) return true;
      }
      if (key === 'backend') {
        const hasBeDev = !!task.beDeveloper;
        const hasBeTitle = task.title.toLowerCase().includes('be') || task.title.toLowerCase().includes('backend');
        if (!hasBeDev && !hasBeTitle) return true;
      }
    }
    return false;
  }

  // Glow class selector
  getColumnColorClass(status: TaskStatus, columnName: string): string {
    const nameLower = (columnName || '').toLowerCase();
    if (nameLower.includes('todo') || status === TaskStatus.Todo) {
      return 'status-todo';
    }
    if (nameLower.includes('progress') || status === TaskStatus.InProgress) {
      return 'status-progress';
    }
    if (nameLower.includes('review') || status === TaskStatus.Review) {
      return 'status-review';
    }
    if (nameLower.includes('qa') || nameLower.includes('done') || status === TaskStatus.Done) {
      return 'status-done';
    }
    if (nameLower.includes('block') || status === TaskStatus.Blocked) {
      return 'status-blocked';
    }
    return 'status-todo';
  }

  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'priority-low';
      case TaskPriority.Medium: return 'priority-medium';
      case TaskPriority.High: return 'priority-high';
      case TaskPriority.Critical: return 'priority-critical';
      default: return '';
    }
  }

  getPriorityLabel(priority: TaskPriority): string {
    return TaskPriority[priority] ?? String(priority);
  }

  getPriorityIcon(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'keyboard_arrow_down';
      case TaskPriority.Medium: return 'drag_handle';
      case TaskPriority.High: return 'keyboard_arrow_up';
      case TaskPriority.Critical: return 'double_arrow';
      default: return 'help';
    }
  }

  getIssueTypeIcon(type: IssueType): string {
    switch (type) {
      case IssueType.Story: return 'bookmark';
      case IssueType.Task: return 'check_box';
      case IssueType.Bug: return 'bug_report';
      case IssueType.SubTask: return 'subdirectory_arrow_right';
      default: return 'description';
    }
  }

  getIssueTypeClass(type: IssueType): string {
    switch (type) {
      case IssueType.Story: return 'type-story';
      case IssueType.Task: return 'type-task';
      case IssueType.Bug: return 'type-bug';
      case IssueType.SubTask: return 'type-subtask';
      default: return '';
    }
  }

  getIssueTypeLabel(type: IssueType): string {
    return IssueType[type] ?? String(type);
  }

  getAssigneeColor(id: string | null): string {
    if (!id) return '#94a3b8';
    let hash = 0;
    for (let i = 0; i < id.length; i++) {
      hash = id.charCodeAt(i) + ((hash << 5) - hash);
    }
    const colors = ['#4f46e5', '#06b6d4', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6'];
    return colors[Math.abs(hash) % colors.length];
  }

  isDelayed(task: TaskResponse): boolean {
    if (!task.latestEta || !task.initialEta) return false;
    return new Date(task.latestEta) > new Date(task.initialEta);
  }

  formatDate(dateStr: string | null | undefined): string {
    if (!dateStr) return '—';
    try {
      return new Date(dateStr).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
    } catch {
      return dateStr;
    }
  }

  toggleDrawer(taskId: string, event: MouseEvent): void {
    event.stopPropagation();
    if (this.openDrawers.has(taskId)) {
      this.openDrawers.delete(taskId);
    } else {
      this.openDrawers.add(taskId);
    }
  }

  isDrawerOpen(taskId: string): boolean {
    return this.openDrawers.has(taskId);
  }

  getDrawerDraft(taskId: string, field: string): string {
    const key = `${taskId}:${field}`;
    if (this.drawerDrafts.has(key)) {
      return this.drawerDrafts.get(key) ?? '';
    }
    const task = this.findTask(taskId);
    if (!task) return '';
    switch (field) {
      case 'fe': return task.feDeveloper ?? '';
      case 'be': return task.beDeveloper ?? '';
      case 'qa': return task.qaEngineer ?? '';
      case 'initialEta': return task.initialEta ? task.initialEta.split('T')[0] : '';
      case 'latestEta': return task.latestEta ? task.latestEta.split('T')[0] : '';
      default: return '';
    }
  }

  onDraftChange(taskId: string, field: string, value: string): void {
    this.drawerDrafts.set(`${taskId}:${field}`, value);
  }

  saveDevInfo(task: TaskResponse, event: MouseEvent): void {
    event.stopPropagation();
    this.savingDrawer.set(task.taskId);
    this.savedDrawer.set(null);

    const get = (f: string) => this.getDrawerDraft(task.taskId, f) || null;

    this.taskService.updateTask(task.taskId, {
      title: task.title,
      description: task.description,
      priority: task.priority,
      dueDate: task.dueDate,
      feDeveloper: get('fe'),
      beDeveloper: get('be'),
      qaEngineer: get('qa'),
      initialEta: get('initialEta') || undefined,
      latestEta: get('latestEta') || undefined,
    }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.savingDrawer.set(null);
          this.savedDrawer.set(task.taskId);
          ['fe','be','qa','initialEta','latestEta'].forEach(f => this.drawerDrafts.delete(`${task.taskId}:${f}`));
          this.columns.update(cols =>
            cols.map(col => ({
              ...col,
              tasks: col.tasks.map(t => t.taskId === updated.taskId ? updated : t),
            }))
          );
          setTimeout(() => this.savedDrawer.set(null), 2000);
        },
        error: (err) => {
          this.savingDrawer.set(null);
          this.error.set(getApiErrorMessage(err));
        },
      });
  }

  private findTask(taskId: string): TaskResponse | undefined {
    for (const col of this.columns()) {
      const t = col.tasks.find(t => t.taskId === taskId);
      if (t) return t;
    }
    return undefined;
  }

  constructor() {
    effect(() => {
      const event = this.realtime.latestEvent();
      if (!event) {
        return;
      }

      if (
        event.entityType === 'Task' ||
        event.eventType.includes('Task')
      ) {
        this.loadBoard();
      }
    });
  }

  ngOnInit(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (projectId) {
      void this.realtime.joinProject(projectId);
    }

    this.loadUsers();

    this.teamService.getTeams().subscribe({
      next: (list) => this.teams.set(list),
    });

    this.loadBoard();
  }

  loadUsers(): void {
    this.userService.loadAllUsers();
  }

  getUserDisplayName(userId: string | null): string {
    return this.userService.getUserDisplayName(userId);
  }

  getUserInitials(userId: string | null): string {
    return this.userService.getUserInitials(userId);
  }

  getUserAvatarUrl(userId: string | null): string | null {
    return this.userService.getUserAvatarUrl(userId);
  }

  connectedLists(): string[] {
    return this.columns().map((col) => this.listId(col));
  }

  listId(col: BoardColumnView): string {
    return `column-${col.column.boardColumnId}`;
  }

  createDefaultBoard(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.busy.set(true);
    this.taskService
      .createBoard(projectId, { name: 'Main Board' })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.loadBoard();
        },
        error: (err) => {
          this.busy.set(false);
          this.error.set(getApiErrorMessage(err));
        },
      });
  }

  onDrop(event: CdkDragDrop<TaskResponse[]>, targetColumn: BoardColumnView): void {
    this.hoveredColumnClass.set(null);
    if (event.previousContainer === event.container || this.statusChangeInFlight) {
      return;
    }

    const task = event.item.data as TaskResponse;
    const targetStatus = targetColumn.column.status;

    // Optimistically update UI state using the columns signal
    this.columns.update(cols => {
      return cols.map(col => {
        // Remove task from previous column if it was there
        let tasks = col.tasks.filter(t => t.taskId !== task.taskId);

        // Add task to target column if this is the target column
        if (col.column.boardColumnId === targetColumn.column.boardColumnId) {
          const updatedTask = { ...task, status: targetStatus as TaskStatus };
          const newTasks = [...tasks];
          newTasks.splice(event.currentIndex, 0, updatedTask);
          tasks = newTasks;
        }

        return {
          ...col,
          tasks
        };
      });
    });

    this.statusChangeInFlight = true;
    this.taskService
      .changeStatus(task.taskId, {
        status: targetStatus as TaskStatus,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedTask) => {
          this.statusChangeInFlight = false;
          if (updatedTask) {
            this.columns.update(cols =>
              cols.map(col => ({
                ...col,
                tasks: col.tasks.map(t => t.taskId === updatedTask.taskId ? updatedTask : t),
              }))
            );
          }
        },
        error: (err) => {
          this.statusChangeInFlight = false;
          this.error.set(getApiErrorMessage(err));
          this.loadBoard(); // Revert to database state on failure
        },
      });
  }

  private loadBoard(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.loading.set(true);
    this.taskService
      .getBoardView(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (view) => {
          this.board.set(view);
          this.columns.set(
            view.columns.map((col) => ({
              ...col,
              tasks: [...col.tasks],
            })),
          );
          this.loading.set(false);
        },
        error: (err) => {
          if (err.status === 404) {
            this.board.set(null);
            this.columns.set([]);
            this.error.set(null);
          } else {
            this.error.set(getApiErrorMessage(err));
          }
          this.loading.set(false);
        },
      });
  }
}
