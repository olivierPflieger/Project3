import { faker } from '@faker-js/faker'

describe('Register tests', () => {
   
  it('Register new user should work', () => {
    
    const url = Cypress.config('baseUrl');
    cy.log('url : ' + url)

    // fake datas    
    const email = faker.internet.email()
    const password = faker.internet.password()
    cy.log('fake user : ' + email + ', ' + password)

    // register new user
    cy.visit('/register')
    cy.get('[data-cy="register-input-email"]').type(email)
    cy.get('[data-cy="register-input-password"]').type(password)
    cy.get('[data-cy="register-input-confirmPassword"]').type(password)
    cy.get('[data-cy="register-button-submit"]').click()

    // assert
    cy.get('[data-cy="register-div-alert"]').should('contain.text', 'Inscription réussie !')     
  }) 

  it('Register new user should work, then register the same user should return an error', () => {
    
    const url = Cypress.config('baseUrl');
    cy.log('url : ' + url)

    // fake datas    
    const email = faker.internet.email()
    const password = faker.internet.password()
    cy.log('fake user : ' + email + ', ' + password)

    // register new user
    cy.visit('/register')
    cy.get('[data-cy="register-input-email"]').type(email)
    cy.get('[data-cy="register-input-password"]').type(password)
    cy.get('[data-cy="register-input-confirmPassword"]').type(password)
    cy.get('[data-cy="register-button-submit"]').click()

    // assert
    cy.get('[data-cy="register-div-alert"]').should('contain.text', 'Inscription réussie !') 
    
    // register the same user again
    cy.visit('/register')
    cy.get('[data-cy="register-input-email"]').type(email)
    cy.get('[data-cy="register-input-password"]').type(password)
    cy.get('[data-cy="register-input-confirmPassword"]').type(password)
    cy.get('[data-cy="register-button-submit"]').click()

    // assert
    cy.get('[data-cy="register-div-alert"]').should('contain.text', 'Un utilisateur existe déjà avec cet email')
    
  }) 

  it('Register new user with wrong datas should display an error', () => {
    
    const url = Cypress.config('baseUrl');
    cy.log('url : ' + url)

    // fake datas    
    const email = 'bad-email'
    const password = 'short'
    const confirmPassword = 'different'
    cy.log('fake user : ' + email + ', ' + password)

    // register new user
    cy.visit('/register')
    cy.get('[data-cy="register-input-email"]').type(email)
    cy.get('[data-cy="register-input-password"]').type(password)
    cy.get('[data-cy="register-input-confirmPassword"]').type(confirmPassword)
    cy.get('[data-cy="register-button-submit"]').click()

    // assert
    cy.get('[data-cy="invalid-feedback-email"]').should('contain.text', 'Veuillez saisir une adresse email valide')
    cy.get('[data-cy="invalid-feedback-password"]').should('contain.text', 'Veuillez saisir un mot de passe d\'au moins 8 caractères')
    cy.get('[data-cy="invalid-feedback-passwordMismatch"]').should('contain.text', 'Les mots de passe ne correspondent pas')
  }) 

  it('Register new user without datas should display an error', () => {
    
    const url = Cypress.config('baseUrl');
    cy.log('url : ' + url)
    
    cy.visit('/register')
    cy.get('[data-cy="register-button-submit"]').click()

    // assert
    cy.get('[data-cy="invalid-feedback-email"]').should('contain.text', 'Veuillez saisir une adresse email')
    cy.get('[data-cy="invalid-feedback-password"]').should('contain.text', 'Veuillez saisir votre mot de passe')
    cy.get('[data-cy="invalid-feedback-confirmPassword"]').should('contain.text', 'Veuillez ressaisir votre mot de passe')
  })
  
})