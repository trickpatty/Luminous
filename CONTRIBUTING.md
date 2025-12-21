# Contributing to Luminous

Thank you for your interest in contributing to Luminous! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Process](#development-process)
- [Pull Request Process](#pull-request-process)
- [Style Guidelines](#style-guidelines)
- [Community](#community)

---

## Code of Conduct

This project adheres to a [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

---

## How Can I Contribute?

### Reporting Bugs

Before creating a bug report, please check existing issues to avoid duplicates.

When filing a bug report, include:

- **Clear title** describing the issue
- **Steps to reproduce** the behavior
- **Expected behavior** vs. actual behavior
- **Screenshots or recordings** if applicable
- **Environment details**:
  - OS and version
  - Display size and resolution
  - Browser/runtime version
  - Hardware specifications

Use this template:

```markdown
## Bug Description
A clear description of what the bug is.

## Steps to Reproduce
1. Go to '...'
2. Click on '...'
3. Scroll down to '...'
4. See error

## Expected Behavior
What you expected to happen.

## Actual Behavior
What actually happened.

## Screenshots
If applicable, add screenshots.

## Environment
- OS: [e.g., Ubuntu 22.04]
- Display: [e.g., 27" 1080x1920 portrait]
- Node version: [e.g., 20.10.0]
- Browser: [e.g., Chromium 120]
```

### Suggesting Features

We welcome feature suggestions! Before suggesting:

1. Check if the feature aligns with our [Design Principles](README.md#design-principles)
2. Search existing issues and discussions
3. Consider if it benefits the typical household use case

When suggesting a feature:

- **Describe the problem** you're trying to solve
- **Explain your proposed solution**
- **Consider alternatives** you've thought about
- **Provide examples** or mockups if possible

### Contributing Code

We especially welcome contributions in these areas:

- UI/UX improvements for large portrait displays
- Accessibility enhancements
- Reliability and startup hardening
- Documentation and setup guides
- Performance optimizations
- Test coverage improvements

---

## Development Process

### 1. Fork and Clone

```bash
# Fork the repository on GitHub, then:
git clone https://github.com/YOUR_USERNAME/Luminous.git
cd Luminous
git remote add upstream https://github.com/trickpatty/Luminous.git
```

### 2. Create a Branch

Branch names should be descriptive:

```bash
# Feature
git checkout -b feature/calendar-week-view

# Bug fix
git checkout -b fix/task-completion-state

# Documentation
git checkout -b docs/setup-guide

# Refactoring
git checkout -b refactor/component-extraction
```

### 3. Development Workflow

Follow **Documentation-Driven Design**:

1. **Document First**: Write or update documentation for your change
2. **Write Tests**: Create tests that define expected behavior (TDD)
3. **Implement**: Write code to pass the tests
4. **Refactor**: Clean up while keeping tests green
5. **Review**: Self-review against our standards

### 4. Commit Your Changes

Follow [Conventional Commits with Gitmoji](CLAUDE.md#git-workflow--commit-standards):

```bash
# Good commits
✨ feat(calendar): add week view navigation
🐛 fix(tasks): resolve completion state persistence
📝 docs(contributing): add development workflow section
✅ test(calendar): add unit tests for date utilities

# Bad commits (avoid these)
"fixed stuff"
"WIP"
"updates"
```

### 5. Keep Your Branch Updated

```bash
git fetch upstream
git rebase upstream/main
```

### 6. Push and Create PR

```bash
git push origin your-branch-name
```

Then create a Pull Request on GitHub.

---

## Pull Request Process

### Before Submitting

Ensure your PR:

- [ ] Follows the [style guidelines](#style-guidelines)
- [ ] Includes tests for new functionality
- [ ] Updates relevant documentation
- [ ] Passes all existing tests
- [ ] Has no linting errors
- [ ] Includes a clear description

### PR Template

```markdown
## Description
Brief description of changes.

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to change)
- [ ] Documentation update
- [ ] Refactoring (no functional changes)

## Related Issues
Fixes #(issue number)

## How Has This Been Tested?
Describe the tests you ran.

## Checklist
- [ ] My code follows the project style guidelines
- [ ] I have performed a self-review
- [ ] I have commented complex code
- [ ] I have updated documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests proving my fix/feature works
- [ ] New and existing tests pass locally
- [ ] Any dependent changes have been merged
```

### Review Process

1. **Automated Checks**: CI must pass
2. **Code Review**: At least one maintainer review required
3. **Documentation Review**: Ensure docs are updated
4. **Testing**: Verify on representative hardware if possible

### After Merge

- Delete your branch
- Update your fork's main branch
- Celebrate your contribution! 🎉

---

## Style Guidelines

### Code Standards

Please refer to [CLAUDE.md](CLAUDE.md) for detailed coding standards including:

- DRY, SOLID, and DDD principles
- Modular and componentized architecture
- Testing requirements
- Accessibility standards

### Commit Messages

Use Conventional Commits with Gitmoji:

```
<gitmoji> <type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

See [CLAUDE.md - Commit Standards](CLAUDE.md#git-workflow--commit-standards) for the complete reference.

### Documentation Style

- Use clear, concise language
- Include code examples where helpful
- Keep the target audience in mind (household users, developers)
- Use proper Markdown formatting
- Add screenshots for UI-related documentation

### Code Formatting

- Follow the project's linting configuration
- Run formatters before committing
- Use meaningful variable and function names
- Keep functions focused and small
- Comment complex logic, not obvious code

---

## Testing Guidelines

### Test Requirements

All contributions should include appropriate tests:

- **Unit Tests**: For individual functions and components
- **Integration Tests**: For feature workflows
- **Accessibility Tests**: For UI components
- **Visual Tests**: For UI changes (if applicable)

### Running Tests

```bash
# Run all tests
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage

# Run specific test file
npm test -- path/to/test
```

### Writing Good Tests

```javascript
// Good test example
describe('TaskList', () => {
  describe('when marking a task complete', () => {
    it('should update the task status', () => {
      // Arrange
      const task = createTask({ status: 'pending' });

      // Act
      const result = markComplete(task);

      // Assert
      expect(result.status).toBe('completed');
    });

    it('should record the completion timestamp', () => {
      // ...
    });
  });
});
```

---

## Community

### Getting Help

- **Questions**: Open a GitHub Discussion
- **Bugs**: Open a GitHub Issue
- **Features**: Open a GitHub Discussion first

### Recognition

Contributors are recognized in:

- The project's contributors list
- Release notes for significant contributions
- Special acknowledgments for major features

---

## License Agreement

By contributing to Luminous, you agree that your contributions will be licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

This means:
- Your contributions remain open source
- Derivative works must also be open source
- The community benefits from all improvements

---

## Thank You!

Every contribution makes Luminous better for families everywhere. Whether it's fixing a typo, improving documentation, or adding a major feature — we appreciate your time and effort.

Together, we're building a calmer way for households to stay connected.
